using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services.GameResources;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public sealed partial class StateCategoryLeafVo(string key, string value, GameValueType type, NodeVo? parent)
    : LeafVo(key, value, type, parent)
{
    public override string Value
    {
        get => LeafValue;
        set
        {
            SetProperty(ref LeafValue, value);
            OnPropertyChanged(nameof(StateCategoryUiDescription));
        }
    }

    public string StateCategoryUiDescription => GetStateCategoryUiDescription();

    public static IEnumerable<StateCategory> StateCategories => StateCategoryService.StateCategories;
    private static readonly StateCategoryService StateCategoryService = App
        .Current.Services.GetRequiredService<GameResourcesService>()
        .StateCategory;

    private string GetStateCategoryUiDescription()
    {
        return StateCategoryService.TryGetValue(Value, out var stateCategory)
            ? $"{stateCategory.TypeNameDescription} 建筑槽位 [{stateCategory.LocalBuildingSlots}]"
            : $"未知的 {Key}";
    }
}
