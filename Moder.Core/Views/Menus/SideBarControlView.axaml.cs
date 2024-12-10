using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Messages;
using Moder.Core.Models;
using Moder.Core.ViewsModel.Menus;

namespace Moder.Core.Views.Menus;

public partial class SideBarControlView : UserControl
{
    // private FAMenuFlyout _menuFlyout = new();
    private FAMenuFlyout _menuFlyout;

    public SideBarControlView()
    {
        InitializeComponent();
        _menuFlyout = Resources["Flyout"] as FAMenuFlyout ?? throw new InvalidOperationException();
        // _menuFlyout.Items.Add();
        // var a = new MenuFlyoutItem
        // {
        //     Text = "Open"
        //     // IconSource = new SymbolIcon(Symbol.OpenFile)
        // };

        DataContext = App.Services.GetRequiredService<SideBarControlViewModel>();

        FileTreeView.AddHandler(PointerPressedEvent, FileTreeView_OnPointerPressed, RoutingStrategies.Tunnel);
    }

    private void FileTreeView_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type != PointerType.Mouse)
        {
            return;
        }

        var point = e.GetCurrentPoint(FileTreeView);

        if (point.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            var item = ((Control?)e.Source)?.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
            if (item is not null)
            {
                _menuFlyout.ShowAt(item);
            }
            e.Handled = true;
        }
    }

    private void TreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
        {
            return;
        }

        if (e.AddedItems[0] is not SystemFileItem item)
        {
            return;
        }

        if (item.IsFolder)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new OpenFileMessage(item));
    }
}
