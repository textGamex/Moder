using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moder.Core.Extensions;
using Moder.Core.Parser;

namespace Moder.Core.Services.GameResources;

/// <summary>
/// 游戏内定义的资源
/// </summary>
/// <remarks>
/// Resource 现在指代的是游戏资源, 游戏内的只好用 Ore 来代替了.
/// </remarks>
/// 单例模式
public sealed class OreService
{
    private readonly FrozenSet<string> _resources;
    private readonly ILogger<OreService> _logger;

    public OreService(IEnumerable<string> filePaths)
    {
        _logger = App.Current.Services.GetRequiredService<ILogger<OreService>>();
        var set = new HashSet<string>(6);

        foreach (var filePath in filePaths)
        {
            if (!TextParser.TryParse(filePath, out var rootNode, out var error))
            {
                _logger.LogParseError(error);
                continue;
            }

            if (rootNode.TryGetChild("resources", out var resourcesNode))
            {
                foreach (var resource in resourcesNode.Nodes)
                {
                    set.Add(resource.Key);
                }
            }
            else
            {
                _logger.LogWarning("未找到 resources 节点");
            }
        }

        _resources = set.ToFrozenSet();
    }

    public bool Contains(string resource) => _resources.Contains(resource);
}
