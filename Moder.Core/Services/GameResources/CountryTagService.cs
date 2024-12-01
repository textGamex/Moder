using System.Collections.Frozen;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.Process;

namespace Moder.Core.Services.GameResources;

public sealed class CountryTagService : CommonResourcesService<CountryTagService, FrozenSet<string>>
{
    /// <summary>
    /// 在游戏内注册的国家标签
    /// </summary>
    public IReadOnlyCollection<string> CountryTags => _countryTagsLazy.Value;

    private Lazy<IReadOnlyCollection<string>> _countryTagsLazy;

    public CountryTagService()
        : base(Path.Combine(Keywords.Common, "country_tags"), WatcherFilter.Text)
    {
        _countryTagsLazy = new Lazy<IReadOnlyCollection<string>>(GetCountryTags);
        OnResourceChanged += (_, _) =>
        {
            _countryTagsLazy = new Lazy<IReadOnlyCollection<string>>(GetCountryTags);
            Log.Debug("Country tags changed, 已重置");
        };
    }

    private string[] GetCountryTags()
    {
        return Resources.Values.SelectMany(set => set.Items).ToArray();
    }

    protected override FrozenSet<string>? ParseFileToContent(Node rootNode)
    {
        var leaves = rootNode.Leaves.ToArray();
        // 不加载临时标签
        if (
            Array.Exists(
                leaves,
                leaf =>
                    leaf.Key.Equals("dynamic_tags", StringComparison.OrdinalIgnoreCase)
                    && leaf.ValueText.Equals("yes", StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return null;
        }
        
        var countryTags = new HashSet<string>(leaves.Length);
        foreach (var leaf in leaves)
        {
            var countryTag = leaf.Key;
            // 国家标签长度必须为 3
            if (countryTag.Length != 3)
            {
                continue;
            }
            countryTags.Add(countryTag);
        }
        return countryTags.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }
}
