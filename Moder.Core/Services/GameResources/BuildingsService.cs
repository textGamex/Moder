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
            Logger.Warn("buildings node not found");
            return null;
        }

        var buildings = ParseBuildingNodeToDictionary(buildingsNode.Nodes);
        return buildings;
    }

    private static FrozenDictionary<string, BuildingInfo> ParseBuildingNodeToDictionary(
        IEnumerable<Node> buildingNodes
    )
    {
        var buildings = new Dictionary<string, BuildingInfo>(8);
        foreach (var buildingNode in buildingNodes)
        {
            byte? maxLevel = null;
            foreach (var buildingPropertyLeaf in buildingNode.Leaves)
            {
                if (buildingPropertyLeaf.Key.Equals("max_level", StringComparison.OrdinalIgnoreCase))
                {
                    if (byte.TryParse(buildingPropertyLeaf.ValueText, out var value))
                    {
                        maxLevel = value;
                    }
                    break;
                }
            }

            buildings[buildingNode.Key] = new BuildingInfo(buildingNode.Key, maxLevel);
        }

        return buildings.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }
}
