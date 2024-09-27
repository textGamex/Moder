using ParadoxPower.Parser;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public sealed partial class CountryTagLeafVo(string key, Types.Value value, NodeVo? parent) : LeafVo(key, value, parent)
{
	public override string Value
	{
		get => LeafValue;
		set
		{
			SetProperty(ref LeafValue, value);
			OnPropertyChanged(nameof(CountryName));
		}
	}

	public string CountryName => LocalisationService.GetValue(Value);
}