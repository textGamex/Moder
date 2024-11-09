using Microsoft.UI.Xaml.Controls;
using Moder.Core.ViewsModels.Game;

namespace Moder.Core.Views.Game;

public sealed partial class TraitsSelectionWindowView
{
    public TraitsSelectionWindowViewModel ViewModel { get; }

    public TraitsSelectionWindowView(TraitsSelectionWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.TraitsModifierDescription = TraitsModifierDescriptionTextBlock.Inlines;
    }

    private void TraitsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.UpdateModifiersDescription(e);
    }
}
