using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Models.Vo;
using Moder.Core.Services.GameResources.Modifiers;
using Moder.Core.ViewsModel.Game;
using Moder.Language.Strings;
using NLog;

namespace Moder.Core.Views.Game;

public sealed partial class TraitSelectionWindowView : Window
{
    public IEnumerable<TraitVo> SelectedTraits =>
        _viewModel.Traits.SourceCollection.Cast<TraitVo>().Where(traitVo => traitVo.IsSelected);

    private readonly IBrush _pointerOverBrush;
    private readonly TraitSelectionWindowViewModel _viewModel;
    // private Flyout _modifierDescriptionFlyout;

    // private readonly Timer _showModifierToolTipTimer;
    private readonly ModifierDisplayService _modifierDisplayService;

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public TraitSelectionWindowView()
    {
        InitializeComponent();

        _modifierDisplayService = App.Services.GetRequiredService<ModifierDisplayService>();
        // _modifierDescriptionFlyout = new Flyout { Placement = PlacementMode.Bottom };
        _viewModel = App.Services.GetRequiredService<TraitSelectionWindowViewModel>();
        DataContext = _viewModel;

        // 因为在选择特质时, 用户无法改变主题, 所以可以直接缓存 Brush
        if (
            App.Current.Styles.TryGetResource(
                "SystemControlHighlightListLowBrush",
                ActualThemeVariant,
                out var value
            ) && value is IBrush brush
        )
        {
            _pointerOverBrush = brush;
        }
        else
        {
            _pointerOverBrush = Brushes.WhiteSmoke;
        }
    }

    public void SyncSelectedTraits(IEnumerable<TraitVo> selectedTraits)
    {
        _viewModel.SyncSelectedTraits(selectedTraits);
    }

    private void Border_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        var border = (Border?)sender;
        if (border is null)
        {
            return;
        }

        if (border.Tag is not TraitVo traitVo)
        {
            Log.Error("无法从 Tag 中获取 TraitVo");
            return;
        }

        border.Background = _pointerOverBrush;

        if (ToolTip.GetTip(border) is not null)
        {
            return;
        }

        var toolTip = new TextBlock { Inlines = [], FontSize = 15 };
        var inlines = _modifierDisplayService.GetModifierDescription(traitVo.Trait.AllModifiers);
        if (inlines.Count == 0)
        {
            toolTip.Inlines.Add(new Run { Text = Resource.CharacterEditor_None });
        }
        else
        {
            toolTip.Inlines.AddRange(inlines);
        }
        ToolTip.SetTip(border, toolTip);
    }

    private void Border_OnPointerExited(object? sender, PointerEventArgs e)
    {
        var border = (Border?)sender;
        if (border is null)
        {
            return;
        }

        border.Background = Brushes.Transparent;
    }

    private void Border_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var border = (Border?)sender;
        if (border?.Tag is not TraitVo traitVo)
        {
            Log.Error("无法从 Tag 中获取 TraitVo");
            return;
        }

        if (traitVo.IsSelected)
        {
            traitVo.IsSelected = false;
            _viewModel.UpdateModifiersDescriptionOnRemove(traitVo);
        }
        else
        {
            traitVo.IsSelected = true;
            _viewModel.UpdateModifiersDescriptionOnAdd(traitVo);
        }
    }
}
