using System.Collections;
using System.Diagnostics;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Moder.Core.Controls;
using Moder.Core.Models;
using Moder.Core.Models.Vo;
using Moder.Core.Services.GameResources;
using Moder.Core.ViewsModels.Game;
using Vanara.PInvoke;
using Windows.Foundation;
using WinUIEx;

namespace Moder.Core.Views.Game;

public sealed partial class StateFileControlView : IFileView
{
    public string Title => ViewModel.Title;
    public string FullPath => ViewModel.FullPath;
    public StateFileControlViewModel ViewModel => (StateFileControlViewModel)DataContext;

    private readonly GameResourcesService _gameResourcesService;
    private readonly DispatcherTimer _timer;

    public StateFileControlView(StateFileControlViewModel model, GameResourcesService gameResourcesService)
    {
        _gameResourcesService = gameResourcesService;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _timer.Tick += (_, _) =>
        {
            User32.GetCursorPos(out var point);
            User32.ScreenToClient(App.Current.MainWindow.GetWindowHandle(), ref point);

            var resolutionScale = MainTreeView.XamlRoot.RasterizationScale;
            var elements = VisualTreeHelper.FindElementsInHostCoordinates(
                new Point(point.X / resolutionScale, point.Y / resolutionScale),
                MainTreeView
            );
            if (
                elements.Any(element =>
                {
                    if (element is BaseLeaf)
                    {
                        return true;
                    }

                    if (element is TreeViewItem { Content: LeafValuesVo or LeafVo })
                    {
                        return true;
                    }

                    return false;
                })
            )
            {
                MainTreeView.CanReorderItems = false;
            }
            else
            {
                MainTreeView.CanReorderItems = true;
            }
        };

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

    private void AutoSuggestBox_OnSuggestionChosen(
        AutoSuggestBox sender,
        AutoSuggestBoxSuggestionChosenEventArgs args
    )
    {
        Debug.Assert(args.SelectedItem is StateCategory);

        var stateCategory = (StateCategory)args.SelectedItem;
        sender.Text = stateCategory.TypeName;
    }

    private void CountryTagAutoSuggestBox_OnTextChanged(
        AutoSuggestBox sender,
        AutoSuggestBoxTextChangedEventArgs args
    )
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
            .OrderByDescending(countryTag =>
                countryTag.StartsWith(query, StringComparison.CurrentCultureIgnoreCase)
            )
            .ToArray();
    }

    private void TreeView_OnDragItemsCompleted(TreeView sender, TreeViewDragItemsCompletedEventArgs args)
    {
        _timer.Stop();
        // _logger.LogDebug("DragItemsCompleted");
    }

    private void TreeView_OnDragItemsStarting(TreeView sender, TreeViewDragItemsStartingEventArgs args)
    {
        _timer.Start();

        // TODO: 不能加入到 Leaf, LeafValue 中 |  加入一个 Node, 离开一个 Node, 调整位置
    }
}
