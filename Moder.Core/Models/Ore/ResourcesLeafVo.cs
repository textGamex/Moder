using Moder.Core.Services.GameResources;
using ParadoxPower.Parser;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public sealed partial class ResourcesLeafVo(string key, Types.Value value, NodeVo? parent) : IntLeafVo(key, value, parent)
{
	public string Name => LocalisationService.GetValue($"state_resource_{Key}");

	private static readonly OreService OreService = GameResourcesService.OreService;
}