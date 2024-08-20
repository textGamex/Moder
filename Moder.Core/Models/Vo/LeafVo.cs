using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Moder.Core.Models.Vo;

public partial class LeafVo : ObservableObject
{
	public string Key { get; private set; }
	public bool IsChanged { get; set; }

	[ObservableProperty]
	private string _value;

	public LeafVo(string key, string value)
	{
		Key = key;
		_value = value;
	}

	protected override void OnPropertyChanging(PropertyChangingEventArgs e)
	{
		IsChanged = true;
		base.OnPropertyChanging(e);
	}
}