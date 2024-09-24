using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services;
using ParadoxPower.Parser;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public sealed partial class StateCategoryLeafVo(string key, Types.Value value, NodeVo? parent)
    : LeafVo(key, value, parent)
{
    public override string Value
    {
        get => _value;
        set
        {
            SetProperty(ref _value, value);
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
            ? $"建筑槽位 [{stateCategory.LocalBuildingSlots}]"
            : $"未知的 {Key}";
    }
}
