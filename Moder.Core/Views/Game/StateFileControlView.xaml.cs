using System.Collections;
using System.Diagnostics;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Models;
using Moder.Core.Services.GameResources;
using Moder.Core.ViewsModels.Game;

namespace Moder.Core.Views.Game;

public sealed partial class StateFileControlView : IFileView
{
    public string Title => ViewModel.Title;
    public string FullPath => ViewModel.FullPath;
    public StateFileControlViewModel ViewModel => (StateFileControlViewModel)DataContext;

    private readonly GameResourcesService _gameResourcesService;

    public StateFileControlView(StateFileControlViewModel model, GameResourcesService gameResourcesService)
    {
        _gameResourcesService = gameResourcesService;

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

    private void CountryTagAutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suggestions = SearchCountryTag(sender.Text);

            sender.ItemsSource = suggestions.Length > 0 ? suggestions : ["未找到"];
        }
    }

    private string[] SearchCountryTag(string query)
    {
        var countryTags = _gameResourcesService.CountryTagsService.CountryTags;
        if (string.IsNullOrWhiteSpace(query))
        {
            return countryTags.ToArray();
        }

        var suggestions = new List<string>(16);

        foreach (var countryTag in countryTags)
        {
            if (countryTag.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add(countryTag);
            }
        }

        return suggestions
            .OrderByDescending(countryTag => countryTag.StartsWith(query, StringComparison.CurrentCultureIgnoreCase))
            .ToArray();
    }

    private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
    {
        var box = sender as TextBox;
        box!.Focus(FocusState.Pointer);
    }
}
