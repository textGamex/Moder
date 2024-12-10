using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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
    // BUG: 第一次右键选择菜单项时文件树有可能会滚动到顶部
    // BUG: 右键选中效果会和滚动条重合, 在实现拉伸文件树时尝试修复
    private readonly FAMenuFlyout _contextMenu;
    private TreeViewItem? _lastSelectedTreeViewItem;
    private readonly SolidColorBrush _rightSelectedItemBorderBrush = new(Colors.CornflowerBlue);
    private readonly SolidColorBrush _transparentBorderBrush = new(Colors.Transparent);
    private readonly Thickness _rightSelectedItemThickness = new(0.65);

    public SideBarControlView()
    {
        InitializeComponent();
        _contextMenu = Resources["ContextMenu"] as FAMenuFlyout ?? throw new InvalidOperationException();

        DataContext = App.Services.GetRequiredService<SideBarControlViewModel>();

        FileTreeView.AutoScrollToSelectedItem = true;
        FileTreeView.AddHandler(PointerPressedEvent, FileTreeView_OnPointerPressed, RoutingStrategies.Tunnel);
    }

    private void FileTreeView_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type != PointerType.Mouse)
        {
            return;
        }

        // 无论左键右键都清理上次右键选中的项的选中效果
        ClearTreeViewItemRightSelectEffect();

        var point = e.GetCurrentPoint(FileTreeView);

        if (point.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            e.Handled = true;

            var treeViewItem = ((Control?)e.Source)
                ?.GetVisualAncestors()
                .OfType<TreeViewItem>()
                .FirstOrDefault();
            if (treeViewItem is null)
            {
                return;
            }

            _contextMenu.ShowAt(treeViewItem, true);
            _lastSelectedTreeViewItem = treeViewItem;
            treeViewItem.BorderThickness = _rightSelectedItemThickness;
            treeViewItem.BorderBrush = _rightSelectedItemBorderBrush;
        }
    }

    private void ClearTreeViewItemRightSelectEffect()
    {
        if (_lastSelectedTreeViewItem is not null)
        {
            _lastSelectedTreeViewItem.BorderBrush = _transparentBorderBrush;
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
