using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Extensions;
using Moder.Core.Helper;
using Moder.Core.Models.Game;
using Moder.Core.Services;
using ParadoxPower.Parser;

namespace Moder.Core.Models.Vo;

public partial class LeafVo : ObservableGameValue
{
    public static IEnumerable<StateCategory> StateCategory { get; }

    private static StateCategoryService StateCategoryService { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StateCategoryUiDescription))]
    private string _value;

    // TODO: 可以拆分到子类 StateCategoryLeafVo 中
    public string StateCategoryUiDescription => GetStateCategoryUiDescription();

    public LeafVo(string key, Types.Value value, NodeVo? parent)
        : base(key, parent)
    {
        _value = value.ToRawString();
        Type = value.ToLocalValueType();
    }

    static LeafVo()
    {
        StateCategoryService = App.Current.Services.GetRequiredService<GameResourcesService>().StateCategory;
        StateCategory = StateCategoryService.StateCategories;
    }

    public Types.Value ToRawValue()
    {
        return ValueConverterHelper.ToValueType(Type, Value);
    }

    private string GetStateCategoryUiDescription()
    {
        return StateCategoryService.TryGetValue(Value, out var stateCategory)
            ? $"建筑槽位 [{stateCategory.LocalBuildingSlots}]"
            : $"未知的 {Key}";
    }
}
