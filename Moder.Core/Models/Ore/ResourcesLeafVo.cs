using Moder.Core.Services.GameResources;
using ParadoxPower.Parser;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public sealed partial class ResourcesLeafVo(string key, string value, GameValueType type, NodeVo parent)
    : IntLeafVo(key, value, type, parent)
{
    public string Name => LocalisationService.GetValue($"state_resource_{Key}");

    private static readonly OreService OreService = GameResourcesService.OreService;
}
