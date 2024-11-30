using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services.GameResources;
using Moder.Language.Strings;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public sealed partial class StateCategoryLeafVo : LeafVo
{
    public StateCategoryLeafVo(string key, string value, GameValueType type, NodeVo? parent)
        : base(key, value, type, parent)
    {
        StateCategory.OnResourceChanged += (_, _) =>
            App.Current.DispatcherQueue.TryEnqueue(() => OnPropertyChanged(nameof(StateCategories)));
    }

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

    public IReadOnlyCollection<StateCategory> StateCategories => StateCategory.StateCategories;

    private static readonly StateCategoryService StateCategory =
        App.Current.Services.GetRequiredService<StateCategoryService>();

    private string GetStateCategoryUiDescription()
    {
        return StateCategory.TryGetValue(Value, out var stateCategory)
            ? $"{stateCategory.TypeNameDescription} {Resource.StateFile_BuildingsSlot} [{stateCategory.LocalBuildingSlots}]"
            : $"{Resource.Common_Unknown} {Key}";
    }
}
