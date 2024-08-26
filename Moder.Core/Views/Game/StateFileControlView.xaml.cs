using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.ViewsModels.Game;

namespace Moder.Core.Views.Game;

public sealed partial class StateFileControlView
{
    public StateFileControlViewModel ViewModel => (StateFileControlViewModel)DataContext;

    public StateFileControlView(StateFileControlViewModel model)
    {
        InitializeComponent();
        DataContext = model;
    }

    private async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
	    if (sender is ListView listView)
	    {
		    await listView.SmoothScrollIntoViewWithIndexAsync(listView.SelectedIndex, ScrollItemPlacement.Center, false, true);
	    }
    }
}
