using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services.GameResources;
using Moder.Language.Strings;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public partial class BuildingLeafVo(string key, string value, GameValueType type, NodeVo parent)
    : IntLeafVo(key, value, type, parent)
{
    public string BuildingName => $"{LocalisationService.GetValue(Key)} {Resource.Common_MaxLevel}: {MaxBuildingLevel}";
    public byte MaxBuildingLevel => GetMaxBuildingLevel();

    private static readonly BuildingsService Service =
        App.Current.Services.GetRequiredService<BuildingsService>();

    private byte GetMaxBuildingLevel()
    {
        if (Service.TryGetBuildingInfo(Key, out var buildingInfo) && buildingInfo.MaxLevel.HasValue)
        {
            return buildingInfo.MaxLevel.Value;
        }

        return byte.MaxValue;
    }
}
