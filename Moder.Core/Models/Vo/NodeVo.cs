using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using ParadoxPower.Process;

namespace Moder.Core.Models.Vo;

public partial class NodeVo(string key, NodeVo? parent) : ObservableGameValue(key, parent)
{
	public ObservableCollection<ObservableGameValue> Children { get; } = [];
	public Visibility AddedValueTextBoxVisibility => SelectedVoType == GameVoType.Node ? Visibility.Collapsed : Visibility.Visible;

	[ObservableProperty]
	private string _addedKey = string.Empty;

	[ObservableProperty]
	private string _addedValue = string.Empty;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(AddedValueTextBoxVisibility))]
	private GameVoType? _selectedVoType;

	public void Add(ObservableGameValue child)
	{
		Children.Add(child);
	}

	public void Remove(ObservableGameValue child)
	{
		var isRemoved = Children.Remove(child);
		Debug.Assert(isRemoved, "Failed to remove child from NodeVo.");
	}

	public override Child ToRawChild()
	{
		var node = new Node(Key);
		var children = new List<Child>(Children.Count);
		children.AddRange(Children.Select(child => child.ToRawChild()));
		node.AllArray = children.ToArray();
		return Child.NewNodeChild(node);
	}

	[RelayCommand]
	private void AddChildValue()
	{
		InternalAddValue(AddType.AddChild);
	}

	[RelayCommand]
	private void AddAdjacentValueForNode()
	{
		InternalAddValue(AddType.AddAdjacent);
	}

	private void InternalAddValue(AddType type)
	{
		if (SelectedVoType is null || string.IsNullOrWhiteSpace(AddedKey))
		{
			return;
		}

		if (SelectedVoType == GameVoType.Leaf && string.IsNullOrWhiteSpace(AddedValue))
		{
			return;
		}

		var parent = type switch
		{
			AddType.AddChild => this,
			AddType.AddAdjacent => Parent,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, "无效枚举值")
		};
		if (parent is null)
		{
			return;
		}

		ObservableGameValue child = SelectedVoType switch
		{
			GameVoType.Node => new NodeVo(AddedKey, parent),
			GameVoType.Leaf => ConverterService.GetSpecificLeafVo(AddedKey, AddedValue, parent),
			GameVoType.LeafValues => new LeafValuesVo(AddedKey, [AddedValue], parent),
			_ => throw new ArgumentOutOfRangeException()
		};

		switch (type)
		{
			case AddType.AddChild:
				Children.Insert(0, child);
				break;
			case AddType.AddAdjacent:
				Debug.Assert(Parent is not null, "Parent is null.");
				Parent?.Children.Insert(Parent.Children.IndexOf(this) + 1, child);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(type), type, "无效枚举值");
		}
	}

	private enum AddType
	{
		AddChild,
		AddAdjacent
	}
}
