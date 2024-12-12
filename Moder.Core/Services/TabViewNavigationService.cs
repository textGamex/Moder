using System.Collections.ObjectModel;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Infrastructure;

namespace Moder.Core.Services;

public sealed class TabViewNavigationService
{
    private TabView TabView =>
        _tabView ?? throw new InvalidOperationException("TabViewNavigationService 未初始化");
    private TabView? _tabView;
    private readonly ObservableCollection<TabViewItem> _openedTabFileItems = [];

    public void Initialize(TabView tabView)
    {
        _tabView = tabView;
        tabView.TabItems = _openedTabFileItems;
    }

    public void AddTab(ITabViewItem content)
    {
        var openedTabFileItem = _openedTabFileItems.FirstOrDefault(item =>
        {
            var tabViewItem = item.Content as ITabViewItem;
            return tabViewItem?.Equals(content) == true;
        });

        AddTabCore(openedTabFileItem, () => content);
    }

    public void AddSingleTabFromIoc<TType>()
        where TType : class, ITabViewItem
    {
        var openedTabFileItem = _openedTabFileItems.FirstOrDefault(item => item.Content is TType);

        AddTabCore(openedTabFileItem, () => App.Services.GetRequiredService<TType>());
    }

    private void AddTabCore(TabViewItem? tabViewItem, Func<ITabViewItem> action)
    {
        if (tabViewItem is null)
        {
            var content = action();
            tabViewItem = new TabViewItem { Header = content.Header, Content = content };
            ToolTip.SetTip(tabViewItem, content.ToolTip);

            _openedTabFileItems.Add(tabViewItem);
        }

        TabView.SelectedItem = tabViewItem;
    }

    public bool RemoveTab(TabViewItem content)
    {
        return _openedTabFileItems.Remove(content);
    }
}
