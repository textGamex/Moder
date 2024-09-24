using System.Collections;
using System.Diagnostics;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Models.Game;
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
        // 确保 ItemsSource 不为空, 避免抛出异常
        if (sender is ListView { ItemsSource: ICollection { Count: > 0 } } listView)
        {
            await listView.SmoothScrollIntoViewWithIndexAsync(
                listView.SelectedIndex,
                ScrollItemPlacement.Center,
                false,
                true
            );
        }
    }

    private void AutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        Debug.Assert(args.SelectedItem is StateCategory);

        var stateCategory = (StateCategory)args.SelectedItem;
        sender.Text = stateCategory.TypeName;
    }
}
