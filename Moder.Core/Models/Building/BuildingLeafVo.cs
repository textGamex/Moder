using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services;
using Moder.Core.Services.GameResources;
using ParadoxPower.Parser;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public partial class BuildingLeafVo(string key, Types.Value value, NodeVo? parent) : LeafVo(key, value, parent)
{
    public byte BuildingLevel
    {
        get => _buildingLevel;
        set
        {
            SetProperty(ref _buildingLevel, value);
            Value = value.ToString();
        }
    }
    private byte _buildingLevel = byte.TryParse(value.ToRawString(), out var level) ? level : (byte)0;

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
