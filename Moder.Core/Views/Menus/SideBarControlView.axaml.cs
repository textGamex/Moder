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
    private readonly FAMenuFlyout _contextMenu;

    public SideBarControlView()
    {
        InitializeComponent();
        _contextMenu = Resources["ContextMenu"] as FAMenuFlyout ?? throw new InvalidOperationException();

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
                _contextMenu.ShowAt(item, true);
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
