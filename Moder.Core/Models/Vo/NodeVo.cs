using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace Moder.Core.Models.Vo;

public partial class NodeVo(string key, NodeVo? parent) : ObservableGameValue(key, parent)
{
	public ObservableCollection<ObservableGameValue> Children { get; } = [];

	public void Add(ObservableGameValue child)
	{
		Children.Add(child);
	}

	public void Remove(ObservableGameValue child)
	{
		var isRemoved = Children.Remove(child);
		Debug.Assert(isRemoved, "Failed to remove child from NodeVo.");
	}

	[RelayCommand]
	private void AddChildNode()
	{

	}
}