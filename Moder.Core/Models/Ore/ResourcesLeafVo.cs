using Moder.Core.Services.GameResources;
using ParadoxPower.Parser;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public sealed partial class ResourcesLeafVo(string key, Types.Value value, NodeVo? parent) : LeafVo(key, value, parent)
{
	public int Amount
	{
		get => _amount;
		set
		{
			SetProperty(ref _amount, value);
			Value = value.ToString();
		}
	}
	private int _amount = int.TryParse(value.ToRawString(), out var amount) ? amount : 0;

	public string Name => LocalisationService.GetValue($"state_resource_{Key}");

	private static readonly OreService OreService = GameResourcesService.OreService;
}