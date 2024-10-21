using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Moder.Core.Messages;
using Moder.Core.Services;
using Moder.Core.ViewsModels.Menus;
using Windows.UI;
using NLog;

namespace Moder.Core.Views.Menus;

public sealed partial class SideWorkSpaceControlView : UserControl
{
    public SideWorkSpaceControlViewModel ViewModel => (SideWorkSpaceControlViewModel)DataContext;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private readonly GlobalResourceService _resourceService;
    private TreeViewItem? _lastSelectedItem;

    public SideWorkSpaceControlView(
        SideWorkSpaceControlViewModel model,
        GlobalResourceService resourceService
    )
    {
        _resourceService = resourceService;
        InitializeComponent();

        DataContext = model;

        WeakReferenceMessenger.Default.Register<SyncSideWorkSelectedItemMessage>(
            this,
            (_, message) =>
            {
#if DEBUG
                // 未选中文件且触发此事件时, 是打开了非文件视图, 比如设置标签页
                // 此时 FileTreeView.SelectedItem 和 message.TargetItem 都为 null, 所以我们先做一个判断
                if (FileTreeView.SelectedItem is not null)
                {
                    Debug.Assert(FileTreeView.SelectedItem != message.TargetItem);
                }
#endif
                FileTreeView.SelectedItem = message.TargetItem;
            }
        );
    }

    private void FileTreeView_OnSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        ClearTreeViewItemRightSelectEffect();

        if (args.AddedItems.Count != 1)
        {
            Log.Debug("未选中文件");
            return;
        }

        if (args.AddedItems[0] is SystemFileItem { IsFile: true } file)
        {
            Log.Info("文件: {File}", file.Name);
            // TODO: 这样做只能打开一个文件
            _resourceService.SetCurrentSelectFileItem(file);

            WeakReferenceMessenger.Default.Send(new OpenFileMessage(file));
        }
    }

    private void TreeViewItem_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        ClearTreeViewItemRightSelectEffect();

        var item = (TreeViewItem)sender;
        _lastSelectedItem = item;
        item.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
    }

    private void ClearTreeViewItemRightSelectEffect()
    {
        if (_lastSelectedItem is not null)
        {
            _lastSelectedItem.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }
    }
}
