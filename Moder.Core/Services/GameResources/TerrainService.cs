using System.Collections.Frozen;
using MethodTimer;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.Process;

namespace Moder.Core.Services.GameResources;

/// <summary>
/// 地形资源服务, 在 common/terrain 目录下
/// </summary>
public sealed class TerrainService : CommonResourcesService<TerrainService, FrozenSet<string>>
{
    private Dictionary<string, FrozenSet<string>>.ValueCollection Terrains => Resources.Values;

    /// <summary>
    /// 未在文件中定义的地形
    /// </summary>
    private readonly FrozenSet<string> _unitTerrain;

    [Time("加载地形资源")]
    public TerrainService()
        : base(Path.Combine(Keywords.Common, "terrain"), WatcherFilter.Text)
    {
        //TODO: 从数据库读取
        _unitTerrain = ["fort", "river"];
    }

    public bool Contains(string terrainName)
    {
        if (_unitTerrain.Contains(terrainName))
        {
            return true;
        }

        foreach (var terrain in Terrains)
        {
            if (terrain.Contains(terrainName))
            {
                return true;
            }
        }

        return false;
    }

    protected override FrozenSet<string>? ParseFileToContent(Node rootNode)
    {
        var terrainSet = new HashSet<string>(16, StringComparer.OrdinalIgnoreCase);
        foreach (var child in rootNode.AllArray)
        {
            if (!child.IsNodeChild)
            {
                continue;
            }

            var node = child.node;
            if (StringComparer.OrdinalIgnoreCase.Equals(node.Key, "categories"))
            {
                foreach (var terrainCategory in node.Nodes)
                {
                    terrainSet.Add(terrainCategory.Key);
                }
                return terrainSet.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
            }
        }

        return null;
    }
}
