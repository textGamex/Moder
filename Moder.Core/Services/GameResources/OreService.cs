using Moder.Core.Services.GameResources.Base;
using ParadoxPower.Process;

namespace Moder.Core.Services.GameResources;

/// <summary>
/// 游戏内定义的资源
/// </summary>
/// <remarks>
/// Resource 现在指代的是游戏资源, 游戏内的只好用 Ore 来代替了.
/// </remarks>
/// 单例模式
public sealed class OreService : CommonResourcesService<OreService, string[]>
{
    private const string ResourcesKeyword = "resources";
    private Dictionary<string, string[]>.ValueCollection Ores => Resources.Values;

    public OreService()
        : base(Path.Combine(Keywords.Common, ResourcesKeyword), WatcherFilter.Text) { }

    public bool Contains(string resource)
    {
        foreach (var ore in Ores)
        {
            if (Array.Exists(ore, x => StringComparer.OrdinalIgnoreCase.Equals(x, resource)))
            {
                return true;
            }
        }

        return false;
    }

    protected override string[] ParseFileToContent(Node rootNode)
    {
        // 一般来说, 每个资源文件只会有一个 resources 节点
        var ores = new List<string>(1);

        if (rootNode.TryGetChild(ResourcesKeyword, out var resourcesNode))
        {
            foreach (var resource in resourcesNode.Nodes)
            {
                ores.Add(resource.Key);
            }
        }
        else
        {
            Logger.Warn("未找到 resources 节点");
        }
        return ores.ToArray();
    }
}
