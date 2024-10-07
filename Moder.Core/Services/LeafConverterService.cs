using Moder.Core.Helper;
using Moder.Core.Models;
using Moder.Core.Models.Vo;
using Moder.Core.Services.GameResources;
using ParadoxPower.Parser;
using ParadoxPower.Process;

namespace Moder.Core.Services;

public sealed class LeafConverterService(GameResourcesService gameResourcesService)
{
    private readonly GameResourcesService _gameResourcesService = gameResourcesService;

    // TODO: 写成配置文件
    private static readonly string[] CountryTagKeywords =
    [
        "add_core_of",
        "owner",
        "add_claim_by",
        "controller",
        "transfer_state"
    ];

    public LeafVo GetSpecificLeafVo(string key, string value, NodeVo parentNodeVo)
    {
        return GetSpecificLeafVo(key, value, GameValueTypeConverterHelper.GetTypeForString(value), parentNodeVo);
    }

    public LeafVo GetSpecificLeafVo(string leafKey, string leafValue, GameValueType type, NodeVo parentNodeVo)
    {
        LeafVo leafVo;

        if (
            Array.Exists(
                CountryTagKeywords,
                countryTag => countryTag.Equals(leafKey, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            leafVo = new CountryTagLeafVo(leafKey, leafValue, type, parentNodeVo);
        }
        else if (leafKey.Equals("name", StringComparison.OrdinalIgnoreCase))
        {
            leafVo = new StateNameLeafVo(leafKey, leafValue, type, parentNodeVo);
        }
        else if (parentNodeVo.Key.Equals("resources", StringComparison.OrdinalIgnoreCase))
        {
            leafVo = new ResourcesLeafVo(leafKey, leafValue, type, parentNodeVo);
        }
        else if (leafKey.Equals("state_category", StringComparison.OrdinalIgnoreCase))
        {
            leafVo = new StateCategoryLeafVo(leafKey, leafValue, type, parentNodeVo);
        }
        // 当父节点是 buildings 时, 子节点就可以为建筑物, 在这里, nodeVo 即为 leaf 的父节点
        else if (
            (
                parentNodeVo.Key.Equals("buildings", StringComparison.OrdinalIgnoreCase)
                // province 中的建筑物
                || parentNodeVo.Parent?.Key.Equals("buildings", StringComparison.OrdinalIgnoreCase) == true
            ) && _gameResourcesService.Buildings.Contains(leafKey)
        )
        {
            leafVo = new BuildingLeafVo(leafKey, leafValue, type, parentNodeVo);
        }
        else
        {
            if (type == GameValueType.Int)
            {
                leafVo = new IntLeafVo(leafKey, leafValue, type, parentNodeVo);
            }
            else if (type == GameValueType.Float)
            {
                leafVo = new FloatLeafVo(leafKey, leafValue, type, parentNodeVo);
            }
            else
            {
                leafVo = new LeafVo(leafKey, leafValue, type, parentNodeVo);
            }
        }

        return leafVo;
    }
}
