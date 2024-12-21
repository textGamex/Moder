using System.ComponentModel;
using Avalonia.Collections;
using Avalonia.Controls.Documents;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnumsNET;
using Moder.Core.Infrastructure;
using Moder.Core.Models.Game.Character;
using Moder.Core.Models.Game.Modifiers;
using Moder.Core.Models.Vo;
using Moder.Core.Services;
using Moder.Core.Services.GameResources;
using Moder.Core.Services.GameResources.Modifiers;
using NLog;

namespace Moder.Core.ViewsModel.Game;

public sealed partial class TraitSelectionWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;
    public DataGridCollectionView Traits { get; }
    public InlineCollection TraitsModifierDescription { get; } = [];

    private readonly AppResourcesService _appResourcesService;
    private readonly ModifierDisplayService _modifierDisplayService;
    private readonly ModifierMergeManager _modifierMergeManager = new();
    private readonly ModifierService _modifierService;

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public TraitSelectionWindowViewModel(
        CharacterTraitsService characterTraitsService,
        AppResourcesService appResourcesService,
        ModifierDisplayService modifierDisplayService,
        ModifierService modifierService
    )
    {
        _appResourcesService = appResourcesService;
        _modifierDisplayService = modifierDisplayService;
        _modifierService = modifierService;

        Traits = new DataGridCollectionView(
            characterTraitsService
                .GetAllTraits()
                .Where(FilterTraitsByCharacterType)
                .Select(trait => new TraitVo(trait, characterTraitsService.GetLocalizationName(trait)))
                .ToArray()
        );

        Traits.Filter += FilterTraitsBySearchText;

        Traits.SortDescriptions.Add(
            new DataGridComparerSortDescription(TraitVo.Comparer.Default, ListSortDirection.Ascending)
        );
    }

    private bool FilterTraitsBySearchText(object obj)
    {
        var traitVo = (TraitVo)obj;
        if (string.IsNullOrEmpty(SearchText))
        {
            return true;
        }

        if (traitVo.Trait.AllModifiers.Any(modifier => modifier.Key.Contains(SearchText)))
        {
            return true;
        }

        if (
            traitVo.Trait.AllModifiers.Any(modifier =>
            {
                if (IsContainsSearchTextInLocalizationModifierName(modifier))
                {
                    return true;
                }

                if (modifier is NodeModifier nodeModifier)
                {
                    return nodeModifier.Modifiers.Any(IsContainsSearchTextInLocalizationModifierName);
                }

                return false;
            })
        )
        {
            return true;
        }

        return traitVo.LocalisationName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || traitVo.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsContainsSearchTextInLocalizationModifierName(IModifier modifier)
    {
        return _modifierService.TryGetLocalizationName(modifier.Key, out var modifierName)
            && modifierName.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    private bool FilterTraitsByCharacterType(Trait trait)
    {
        if (_appResourcesService.CurrentSelectedCharacterType == SkillCharacterType.Navy)
        {
            if (trait.Type.HasAnyFlags(TraitType.Navy))
            {
                return true;
            }

            return false;
        }

        if (trait.Type == TraitType.Navy)
        {
            return false;
        }

        // TODO: 暂不支持间谍
        if (trait.Type == TraitType.Operative)
        {
            return false;
        }

        return true;
    }

    [RelayCommand]
    private void ClearTraits()
    {
        foreach (TraitVo trait in Traits.SourceCollection)
        {
            trait.IsSelected = false;
        }
        TraitsModifierDescription.Clear();
        _modifierMergeManager.Clear();
    }

    partial void OnSearchTextChanged(string value)
    {
        Traits.Refresh();
    }

    public void UpdateModifiersDescriptionOnAdd(TraitVo traitVo)
    {
        _modifierMergeManager.AddRange(traitVo.Trait.AllModifiers);
        UpdateModifiersDescriptionCore();
    }

    public void UpdateModifiersDescriptionOnRemove(TraitVo traitVo)
    {
        _modifierMergeManager.RemoveAll(traitVo.Trait.AllModifiers);
        UpdateModifiersDescriptionCore();
    }

    private void UpdateModifiersDescriptionCore()
    {
        var mergedModifiers = _modifierMergeManager.GetMergedModifiers();
        var addedModifiers = _modifierDisplayService.GetDescription(mergedModifiers);

        Dispatcher.UIThread.Post(() =>
        {
            TraitsModifierDescription.Clear();
            TraitsModifierDescription.AddRange(addedModifiers);
        });
    }

    public void SyncSelectedTraits(IEnumerable<TraitVo> selectedTraits)
    {
        // 因为有可能因为特质文件改变导致选择的特质数量发生变化，因此这里需要过滤一下
        var selectedTraitNames = selectedTraits.Select(trait => trait.Name).ToHashSet();
        if (selectedTraitNames.Count == 0)
        {
            return;
        }

        var traitVos = new List<TraitVo>(8);
        foreach (TraitVo trait in Traits.SourceCollection)
        {
            if (selectedTraitNames.Contains(trait.Name))
            {
                trait.IsSelected = true;
                traitVos.Add(trait);
            }
        }
        UpdateModifiersDescriptionOnAdd(traitVos);
    }

    private void UpdateModifiersDescriptionOnAdd(IEnumerable<TraitVo> traitVos)
    {
        _modifierMergeManager.AddRange(traitVos.SelectMany(traitVo => traitVo.Trait.AllModifiers));
        UpdateModifiersDescriptionCore();
    }
}
