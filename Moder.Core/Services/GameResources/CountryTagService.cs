using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moder.Core.Extensions;
using Moder.Core.Parser;

namespace Moder.Core.Services.GameResources;

public sealed class CountryTagService
{
    /// <summary>
    /// 在游戏内注册的国家标签
    /// </summary>
    public IReadOnlyCollection<string> CountryTags => _countryTags;

    private readonly FrozenSet<string> _countryTags;
    private readonly ILogger<CountryTagService> _logger;

    public CountryTagService(IEnumerable<string> filePaths)
    {
        _logger = App.Current.Services.GetRequiredService<ILogger<CountryTagService>>();

        var countryTags = new HashSet<string>(256);
        foreach (var filePath in filePaths)
        {
            if (!TextParser.TryParse(filePath, out var rootNode, out var error))
            {
                _logger.LogParseError(error);
                continue;
            }

            // 不加载临时标签
            if (
                Array.Exists(
                    rootNode.Leaves.ToArray(),
                    leaf =>
                        leaf.Key.Equals("dynamic_tags", StringComparison.OrdinalIgnoreCase)
                        && leaf.ValueText.Equals("yes", StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                continue;
            }

            foreach (var leaf in rootNode.Leaves)
            {
                countryTags.Add(leaf.Key);
            }
        }

        _countryTags = countryTags.ToFrozenSet();
    }
}
