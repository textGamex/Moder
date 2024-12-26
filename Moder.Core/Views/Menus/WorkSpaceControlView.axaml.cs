using System.Diagnostics;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services;

namespace Moder.Core.Views.Menus;

public sealed partial class WorkSpaceControlView : UserControl
{
    private readonly TabViewNavigationService _tabService;

    public WorkSpaceControlView()
    {
        InitializeComponent();

        _tabService = App.Services.GetRequiredService<TabViewNavigationService>();
        _tabService.Initialize(MainTabView);
    }

    private void MainTabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        var isRemoved = _tabService.RemoveTab(args.Tab);
        Debug.Assert(isRemoved);
    }
}
