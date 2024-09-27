using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moder.Core.Models;
using Moder.Core.Parser;

namespace Moder.Core.Services.GameResources;

public sealed class StateCategoryService
{
    public IReadOnlyCollection<StateCategory> StateCategories { get; }

    private readonly FrozenDictionary<string, StateCategory> _stateCategories;

    private static readonly ILogger<StateCategoryService> Logger = App.Current.Services.GetRequiredService<
        ILogger<StateCategoryService>
    >();

    public StateCategoryService(IEnumerable<string> filePaths)
    {
        var stateCategories = new Dictionary<string, StateCategory>();

        // TODO: 抽成基类?
        foreach (var filePath in filePaths)
        {
            if (!TextParser.TryParse(filePath, out var node, out var error))
            {
                Logger.LogError("文件: {path} 解析失败, 错误信息: {message}", error.Filename, error.ErrorMessage);
                continue;
            }

            if (!node.TryGetChild("state_categories", out var stateCategoriesNode))
            {
                Logger.LogWarning("文件: {path} 中未找到 state_categories 节点", filePath);
                continue;
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
                        Logger.LogWarning(
                            "文件: {path} 中 local_building_slots 解析失败, 文本: {Text}",
                            filePath,
                            localBuildingSlotsLeaf.ValueText
                        );
                    }
                }
                else
                {
                    Logger.LogWarning("文件: {path} 中未找到 local_building_slots", filePath);
                }

                stateCategories.Add(typeName, new StateCategory(typeName, localBuildingSlots));
            }
        }

        _stateCategories = stateCategories.ToFrozenDictionary();
        var sortedArray = stateCategories.Values.ToArray();
        Array.Sort(sortedArray, (x, y) => x.LocalBuildingSlots < y.LocalBuildingSlots ? -1 : 1);
        StateCategories = sortedArray;
    }

    public bool ContainsCategory(string categoryName)
    {
        return _stateCategories.ContainsKey(categoryName);
    }

    public bool TryGetValue(string categoryName, [NotNullWhen(true)] out StateCategory? category)
    {
        return _stateCategories.TryGetValue(categoryName, out category);
    }
}
