using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using ParadoxPower.Parser;

namespace Moder.Core.Models.Vo;

public partial class FloatLeafVo(string key, Types.Value value, NodeVo? parent) : LeafVo(key, value, parent)
{
	[ObservableProperty]
	private double _numberValue = double.TryParse(value.ToRawString(), out var number) ? number : 0D;

	partial void OnNumberValueChanged(double value)
	{
		Value = value.ToString(CultureInfo.InvariantCulture);
	}
}