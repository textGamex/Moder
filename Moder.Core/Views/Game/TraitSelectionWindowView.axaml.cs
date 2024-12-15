using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Models;
using Moder.Core.Models.Vo;
using Moder.Core.ViewsModel.Game;

namespace Moder.Core.Views.Game;

public sealed partial class TraitSelectionWindowView : Window
{
    public IEnumerable<TraitVo> SelectedTraits =>
        _viewModel.Traits.SourceCollection.Cast<TraitVo>().Where(traitVo => traitVo.IsSelected);

    private readonly IBrush _pointerOverBrush;
    private readonly TraitSelectionWindowViewModel _viewModel;

    public TraitSelectionWindowView(IEnumerable<TraitVo> selectedTraits)
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<TraitSelectionWindowViewModel>();
        DataContext = _viewModel;
        _viewModel.SyncSelectedTraits(selectedTraits);

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

    private void Border_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        var border = (Border?)sender;
        if (border is null)
        {
            return;
        }

        border.Background = _pointerOverBrush;
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
