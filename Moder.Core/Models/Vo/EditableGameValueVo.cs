using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Moder.Core.Models.Vo;

public sealed partial class EditableGameValueVo(NodeVo parent) : ObservableGameValue("", parent)
{
	public string[] Types => ["Node", "Leaf", "LeafValues"];
	[ObservableProperty]
	private string? _keyword;
	[ObservableProperty]
	private string? _value;

	private readonly NodeVo _parent = parent;

	[RelayCommand]
	private void AddValue()
	{
	}

	[RelayCommand]
	private void Cancel()
	{

	}
}