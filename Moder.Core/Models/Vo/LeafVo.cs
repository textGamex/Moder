using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Helper;
using Moder.Core.Services.GameResources;
using ParadoxPower.Parser;
using ParadoxPower.Process;
using ParadoxPower.Utilities;

namespace Moder.Core.Models.Vo;

public partial class LeafVo : ObservableGameValue
{
	public virtual string Value
	{
		get => LeafValue;
		set => SetProperty(ref LeafValue, value);
	}
	protected string LeafValue;

	protected static readonly LocalisationService LocalisationService;
	protected static readonly GameResourcesService GameResourcesService;

	static LeafVo()
	{
		GameResourcesService = App.Current.Services.GetRequiredService<GameResourcesService>();
		LocalisationService = GameResourcesService.Localisation;
	}

	public LeafVo(string key, string value, GameValueType type, NodeVo? parent)
		: base(key, parent)
	{
		LeafValue = value;
		Type = type;
	}

	public Types.Value ToRawValue()
	{
		return ValueConverterHelper.ToValueType(Type, Value);
	}

	// ~LeafVo()
	// {
	//     Debug.WriteLine($"LeafVo Finalized");
	// }

	public override Child ToRawChild()
	{
		return Child.NewLeafChild(new Leaf(Key, ToRawValue(), Position.Range.Zero, Types.Operator.Equals));
	}
}
