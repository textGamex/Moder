using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Moder.Core.Models;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.Process;

namespace Moder.Core.Services.GameResources;

public sealed class StateCategoryService
    : CommonResourcesService<StateCategoryService, FrozenDictionary<string, StateCategory>>
{
    public IReadOnlyList<StateCategory> StateCategories => _lazyStateCategories.Value;
    private Lazy<IReadOnlyList<StateCategory>> _lazyStateCategories;

    private Dictionary<
        string,
        FrozenDictionary<string, StateCategory>
    >.ValueCollection StateCategoriesResource => Resources.Values;

    public StateCategoryService()
        : base(Path.Combine(Keywords.Common, "state_category"), WatcherFilter.Text)
    {
        _lazyStateCategories = GetStateCategoriesLazy();
        OnResourceChanged += (_, _) =>
        {
            _lazyStateCategories = GetStateCategoriesLazy();
        };
    }

    private Lazy<IReadOnlyList<StateCategory>> GetStateCategoriesLazy()
    {
        return new Lazy<IReadOnlyList<StateCategory>>(() =>
        {
            var sortedArray = StateCategoriesResource.SelectMany(item => item.Values).ToArray();
            Array.Sort(sortedArray, (x, y) => x.LocalBuildingSlots < y.LocalBuildingSlots ? -1 : 1);
            return sortedArray;
        });
    }

    public bool ContainsCategory(string categoryName)
    {
        foreach (var stateCategory in StateCategoriesResource)
        {
            if (stateCategory.ContainsKey(categoryName))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetValue(string categoryName, [NotNullWhen(true)] out StateCategory? category)
    {
        foreach (var stateCategory in StateCategoriesResource)
        {
            if (stateCategory.TryGetValue(categoryName, out category))
            {
                return true;
            }
        }

        category = null;
        return false;
    }

    protected override FrozenDictionary<string, StateCategory>? ParseFileToContent(Node rootNode)
    {
        var stateCategories = new Dictionary<string, StateCategory>(8);
        if (!rootNode.TryGetChild("state_categories", out var stateCategoriesNode))
        {
            Logger.Warn("文件: {FileName} 中未找到 state_categories 节点", rootNode.Position.FileName);
            return null;
        }

        foreach (var typeNode in stateCategoriesNode.Nodes)
        {
            byte? localBuildingSlots = null;
            var typeName = typeNode.Key;

            if (typeNode.TryGetLeaf("local_building_slots", out var localBuildingSlotsLeaf))
            {
                if (byte.TryParse(localBuildingSlotsLeaf.ValueText, out var value))
                {
                    localBuildingSlots = value;
                }
                else
                {
                    Logger.Warn(
                        "文件: {FileName} 中 local_building_slots 解析失败, 文本: {Text}",
                        typeNode.Position.FileName,
                        localBuildingSlotsLeaf.ValueText
                    );
                }
            }
            else
            {
                Logger.Warn("文件: {FileName} 中未找到 local_building_slots", typeNode.Position.FileName);
            }

            stateCategories.Add(typeName, new StateCategory(typeName, localBuildingSlots));
        }

        return stateCategories.ToFrozenDictionary();
    }
}
