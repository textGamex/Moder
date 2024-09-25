using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moder.Core.Extensions;
using Moder.Core.Models;
using Moder.Core.Parser;
using ParadoxPower.Process;

namespace Moder.Core.Services;

public sealed class BuildingsService
{
    private readonly FrozenDictionary<string, BuildingInfo> _buildings;
    private readonly ILogger<BuildingsService> _logger;

    public BuildingsService(IEnumerable<string> filePaths)
    {
        _logger = App.Current.Services.GetRequiredService<ILogger<BuildingsService>>();

        // 预设容量数据来自 1.14.8 版本
        var buildings = new Dictionary<string, BuildingInfo>(16);
        foreach (var filePath in filePaths)
        {
            if (!TextParser.TryParse(filePath, out var rootNode, out var error))
            {
                _logger.LogParseError(error);
                continue;
            }

            if (!rootNode.TryGetChild("buildings", out var buildingsNode))
            {
                _logger.LogWarning("buildings node not found");
                continue;
            }

            ParseBuildingNodeToDictionary(buildingsNode.Nodes, buildings);
        }

        _buildings = buildings.ToFrozenDictionary();
    }

    private static void ParseBuildingNodeToDictionary(
        IEnumerable<Node> buildingNodes,
        Dictionary<string, BuildingInfo> buildings
    )
    {
        foreach (var buildingNode in buildingNodes)
        {
            byte? maxLevel = null;
            foreach (var leaf in buildingNode.Leaves)
            {
                if (leaf.Key.Equals("max_level", StringComparison.OrdinalIgnoreCase))
                {
                    if (byte.TryParse(leaf.ValueText, out var value))
                    {
                        maxLevel = value;
                    }
                    break;
                }
            }
            buildings[buildingNode.Key] = new BuildingInfo(buildingNode.Key, maxLevel);
        }
    }

    public bool Contains(string buildingType) => _buildings.ContainsKey(buildingType);

    public bool TryGetBuildingInfo(string buildingType, [NotNullWhen(true)] out BuildingInfo? buildingInfo) =>
        _buildings.TryGetValue(buildingType, out buildingInfo);
}
