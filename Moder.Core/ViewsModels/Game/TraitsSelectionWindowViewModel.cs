using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using EnumsNET;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Moder.Core.Helper;
using Moder.Core.Models.Character;
using Moder.Core.Services;
using Moder.Core.Services.GameResources;
using NLog;

namespace Moder.Core.ViewsModels.Game;

public sealed partial class TraitsSelectionWindowViewModel : ObservableObject
{
    public InlineCollection? TraitsModifierDescription { get; set; }

    // TODO: /搜索/, /显示修正效果/, 筛选, (图标?)
    public AdvancedCollectionView Traits { get; }

    [ObservableProperty]
    private string _searchText = string.Empty;

    private readonly GlobalResourceService _globalResourceService;
    private readonly ModifierService _modifierService;
    private readonly ModifierMergeManager _modifierMergeManager = new();
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    [RequiresUnreferencedCode("Use AdvancedCollectionView")]
    public TraitsSelectionWindowViewModel(
        LocalisationService localisationService,
        CharacterTraitsService characterTraitsService,
        GlobalResourceService globalResourceService,
        ModifierService modifierService
    )
    {
        _globalResourceService = globalResourceService;
        _modifierService = modifierService;
        // TODO: 影响裁剪, 换个解决方案
        // BUG: 搜索后已选择的会取消选中
        Traits = new AdvancedCollectionView(
            characterTraitsService
                .GetAllTraits()
                .Where(FilterTraitsByCharacterType)
                .Select(trait => new TraitVo(trait, localisationService.GetValue(trait.Name)))
                .ToArray()
        );
        Traits.Filter += FilterTraitsBySearchText;
    }

    partial void OnSearchTextChanged(string value)
    {
        Traits.RefreshFilter();
    }

    private bool FilterTraitsBySearchText(object obj)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var traitVo = (TraitVo)obj;
        return traitVo.LocalisationName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || traitVo.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    private bool FilterTraitsByCharacterType(Trait trait)
    {
        if (_globalResourceService.CurrentSelectSelectSkillType == CharacterSkillType.Navy)
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

    public void UpdateModifiersDescription(SelectionChangedEventArgs args)
    {
        if (args.RemovedItems.Count != 0)
        {
            var removedTraitVo = (TraitVo)args.RemovedItems[0];
            _modifierMergeManager.RemoveAll(
                removedTraitVo.Trait.Modifiers.SelectMany(collection => collection.Modifiers)
            );
        }

        if (args.AddedItems.Count != 0)
        {
            var traitVo = (TraitVo)args.AddedItems[0];
            _modifierMergeManager.AddRange(
                traitVo.Trait.Modifiers.SelectMany(collection => collection.Modifiers)
            );
        }

        if (TraitsModifierDescription is null)
        {
            Log.Warn("TraitsModifierDescription is null");
            return;
        }

        var mergedModifiers = _modifierMergeManager.GetMergedModifiers();
        var addedModifiers = _modifierService.GetModifierInlines(mergedModifiers);

        TraitsModifierDescription.Clear();
        foreach (var inline in addedModifiers)
        {
            TraitsModifierDescription.Add(inline);
        }
    }
}

public sealed class TraitVo(Trait trait, string localisationName) : IEquatable<TraitVo>
{
    public string Name => trait.Name;
    public Trait Trait => trait;
    public string LocalisationName => localisationName;

    public bool Equals(TraitVo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TraitVo);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
