using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services.GameResources;
using ParadoxPower.Parser;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public partial class BuildingLeafVo(string key, Types.Value value, NodeVo? parent) : IntLeafVo(key, value, parent)
{
    public string BuildingName => $"{LocalisationService.GetValue(Key)} 最大等级: {MaxBuildingLevel}";
    public byte MaxBuildingLevel => GetMaxBuildingLevel();

    private static readonly BuildingsService Service = App
        .Current.Services.GetRequiredService<GameResourcesService>()
        .Buildings;

    private byte GetMaxBuildingLevel()
    {
        if (Service.TryGetBuildingInfo(Key, out var buildingInfo) && buildingInfo.MaxLevel.HasValue)
        {
            return buildingInfo.MaxLevel.Value;
        }

        return byte.MaxValue;
    }
}
