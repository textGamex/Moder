using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Moder.Core.Models.Vo;
using Moder.Core.ViewsModels.Game;

namespace Moder.Core.Views.Game;

public sealed partial class TraitsSelectionWindowView : IDisposable
{
    public TraitsSelectionWindowViewModel ViewModel { get; }

    private readonly Timer _showModifierToolTipTimer;
    private DependencyObject? _displayTarget;
    private TraitVo? _traitVo;
    private readonly FlyoutShowOptions _flyoutShowOptions =
        new() { ShowMode = FlyoutShowMode.Transient };

    private readonly SolidColorBrush _whiteSmokeBrush = new(Colors.WhiteSmoke);
    private readonly SolidColorBrush _transparentBrush = new(Colors.Transparent);

    public TraitsSelectionWindowView()
    {
        ViewModel = App.Current.Services.GetRequiredService<TraitsSelectionWindowViewModel>();
        InitializeComponent();

        ViewModel.TraitsModifierDescription = TraitsModifierDescriptionTextBlock.Inlines;
        _showModifierToolTipTimer = new Timer(TimeSpan.FromMilliseconds(300)) { AutoReset = false };

        _showModifierToolTipTimer.Elapsed += (_, _) =>
        {
            if (_displayTarget is null || _traitVo is null)
            {
                return;
            }

            App.Current.DispatcherQueue.TryEnqueue(() =>
            {
                ModifierToolTip.Content = _traitVo.Description;
                ModifierToolTip.ShowAt(_displayTarget, _flyoutShowOptions);
            });
        };
    }

    public void Dispose()
    {
        _showModifierToolTipTimer.Dispose();
        ViewModel.Close();
    }

    private void Border_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        var border = (Border)sender;
        border.Background = _whiteSmokeBrush;
        _traitVo = (TraitVo)border.DataContext;
        _displayTarget = border;
        _showModifierToolTipTimer.Start();
    }

    private void Border_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        var border = (Border)sender;
        border.Background = _transparentBrush;
        ModifierToolTip.Hide();
        _showModifierToolTipTimer.Stop();
    }
}
