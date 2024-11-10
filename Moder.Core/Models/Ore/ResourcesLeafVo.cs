using Moder.Core.Services.GameResources;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public sealed partial class ResourcesLeafVo(string key, string value, GameValueType type, NodeVo parent)
    : IntLeafVo(key, value, type, parent)
{
    public string Name => LocalisationService.GetValue($"state_resource_{Key}");
}
