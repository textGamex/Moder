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
    private bool _isTabViewChanged;

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
                AssertNeedSync(message);
                _isTabViewChanged = true;
                FileTreeView.SelectedItem = message.TargetItem;
                Log.Debug("侧边栏同步选中项为: {SelectedItem}", message.TargetItem?.Name);
            }
        );
    }

    [Conditional("DEBUG")]
    private void AssertNeedSync(SyncSideWorkSelectedItemMessage message)
    {
        // 未选中文件且触发此事件时, 是打开了非文件视图, 比如设置标签页
        // 此时 FileTreeView.SelectedItem 和 message.TargetItem 都为 null, 所以我们先做一个判断
        if (FileTreeView.SelectedItem is not null)
        {
            Debug.Assert(FileTreeView.SelectedItem != message.TargetItem);
        }
    }

    private void FileTreeView_OnSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        ClearTreeViewItemRightSelectEffect();
        
        // 当用户切换选中的 FileTreeView 时, 会触发两次 SelectionChanged 事件, 第一次是离开当前选中项, 第二次是进入新选中项
        // 这里不处理第一次的离开事件, 只处理第二次的进入事件
        if (args.AddedItems.Count != 1)
        {
            return;
        }
        
        // 如果是标签页切换, 则仅仅切换 FileTreeView 的选中项, 不做任何多余的操作
        if (_isTabViewChanged)
        {
            _isTabViewChanged = false;
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
