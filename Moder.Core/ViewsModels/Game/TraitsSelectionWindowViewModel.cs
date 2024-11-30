using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Collections;
using EnumsNET;
using Microsoft.UI.Xaml.Documents;
using Moder.Core.Helper;
using Moder.Core.Messages;
using Moder.Core.Models.Character;
using Moder.Core.Models.Vo;
using Moder.Core.Services;
using Moder.Core.Services.GameResources;
using Moder.Language.Strings;
using NLog;

namespace Moder.Core.ViewsModels.Game;

public sealed partial class TraitsSelectionWindowViewModel : ObservableObject
{
    public InlineCollection? TraitsModifierDescription { get; set; }

    // TODO: /搜索/, /显示修正效果/, 筛选, (图标?)
    public AdvancedCollectionView Traits { get; }

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _buttonText = Resource.Common_SelectAll;

    private ushort _selectedTraitCount;
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
        Traits = new AdvancedCollectionView(
            characterTraitsService
                .GetAllTraits()
                .Where(FilterTraitsByCharacterType)
                .Select(trait => new TraitVo(trait, localisationService.GetValue(trait.Name)))
                .ToArray()
        );
        Traits.Filter += FilterTraitsBySearchText;

        WeakReferenceMessenger.Default.Register<SelectedTraitChangedMessage>(
            this,
            (_, message) =>
            {
                if (message.IsAdded)
                {
                    _selectedTraitCount++;
                    UpdateModifiersDescriptionOnAdd(message.Trait);
                }
                else
                {
                    _selectedTraitCount--;
                    UpdateModifiersDescriptionOnRemove(message.Trait);
                }
            }
        );
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

    private void UpdateModifiersDescriptionOnAdd(TraitVo traitVo)
    {
        _modifierMergeManager.AddRange(traitVo.Trait.AllModifiers);
        UpdateModifiersDescriptionCore();
    }

    private void UpdateModifiersDescriptionOnRemove(TraitVo traitVo)
    {
        _modifierMergeManager.RemoveAll(traitVo.Trait.AllModifiers);
        UpdateModifiersDescriptionCore();
    }

    private void UpdateModifiersDescriptionCore()
    {
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

        // 每当选中或删除一个特性时，都会调用此方法, 此时我们需要更新按钮的文本
        ButtonText = _selectedTraitCount == 0 ? Resource.Common_SelectAll : Resource.Common_Clear;
    }

    [RelayCommand]
    private void ClickButton()
    {
        if (_selectedTraitCount == 0)
        {
            // 全选
            foreach (TraitVo trait in Traits)
            {
                if (!trait.IsSelected)
                {
                    trait.IsSelected = true;
                }
            }
        }
        else
        {
            // 清空
            foreach (TraitVo trait in Traits)
            {
                if (trait.IsSelected)
                {
                    trait.IsSelected = false;
                }
            }
        }
    }

    public void Close()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public void SyncSelectedTraits(IEnumerable<TraitVo> selectedTraits)
    {
        var selectedTraitNames = selectedTraits.Select(trait => trait.Name).ToHashSet();
        if (selectedTraitNames.Count == 0)
        {
            return;
        }

        foreach (TraitVo trait in Traits)
        {
            if (selectedTraitNames.Contains(trait.Name))
            {
                trait.IsSelected = true;
            }
        }
    }
}
