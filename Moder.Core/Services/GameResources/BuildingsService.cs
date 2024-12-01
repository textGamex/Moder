using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Moder.Core.Models;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.CSharpExtensions;
using ParadoxPower.Process;

namespace Moder.Core.Services.GameResources;

public sealed class BuildingsService
    : CommonResourcesService<BuildingsService, FrozenDictionary<string, BuildingInfo>>
{
    private Dictionary<string, FrozenDictionary<string, BuildingInfo>>.ValueCollection Buildings =>
        Resources.Values;
    private const string BuildingsKeyword = "buildings";

    public BuildingsService()
        : base(Path.Combine(Keywords.Common, BuildingsKeyword), WatcherFilter.Text) { }

    public bool Contains(string buildingType)
    {
        foreach (var building in Buildings)
        {
            if (building.ContainsKey(buildingType))
            {
                return true;
            }
        }
        return false;
    }

    public bool TryGetBuildingInfo(string buildingType, [NotNullWhen(true)] out BuildingInfo? buildingInfo)
    {
        foreach (var building in Buildings)
        {
            if (building.TryGetValue(buildingType, out buildingInfo))
            {
                return true;
            }
        }

        buildingInfo = null;
        return false;
    }

    protected override FrozenDictionary<string, BuildingInfo>? ParseFileToContent(Node rootNode)
    {
        if (!rootNode.TryGetNode(BuildingsKeyword, out var buildingsNode))
        {
            Log.Warn("buildings node not found");
            return null;
        }

        var buildings = ParseBuildingNode(buildingsNode.Nodes);
        return buildings;
    }

    private FrozenDictionary<string, BuildingInfo> ParseBuildingNode(IEnumerable<Node> buildingNodes)
    {
        var buildings = new Dictionary<string, BuildingInfo>(8);
        foreach (var buildingNode in buildingNodes)
        {
            ParseBuildingNodeToDictionary(buildingNode, buildings);
        }

        return buildings.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private void ParseBuildingNodeToDictionary(
        Node buildingNode,
        Dictionary<string, BuildingInfo> buildings
    )
    {
        byte? maxLevel = null;
        var levelCapNode = buildingNode.Nodes.FirstOrDefault(node =>
            StringComparer.OrdinalIgnoreCase.Equals(node.Key, "level_cap")
        );
        if (levelCapNode is null)
        {
            Log.Warn("建筑 {Building} 的 level_cap node 未找到", buildingNode.Key);
            return;
        }

        foreach (var levelPropertyLeaf in levelCapNode.Leaves)
        {
            if (
                levelPropertyLeaf.Key.Equals("state_max", StringComparison.OrdinalIgnoreCase)
                || levelPropertyLeaf.Key.Equals("province_max", StringComparison.OrdinalIgnoreCase)
            )
            {
                if (byte.TryParse(levelPropertyLeaf.ValueText, out var value))
                {
                    maxLevel = value;
                }
                break;
            }
        }

        if (!maxLevel.HasValue)
        {
            Log.Warn("{Building} 的 最大等级 信息未找到", buildingNode.Key);
        }
        buildings[buildingNode.Key] = new BuildingInfo(buildingNode.Key, maxLevel);
    }
}
