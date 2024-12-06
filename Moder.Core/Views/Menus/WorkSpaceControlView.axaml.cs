using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using Moder.Core.Messages;

namespace Moder.Core.Views.Menus;

public partial class WorkSpaceControlView : UserControl
{
    private readonly ObservableCollection<object> _openedTabFileItems;

    public WorkSpaceControlView()
    {
        InitializeComponent();
        _openedTabFileItems = [];

        MainTabView.TabItems = _openedTabFileItems;
        WeakReferenceMessenger.Default.Register<OpenFileMessage>(
            this,
            (_, message) => _openedTabFileItems.Add(new NotSupportInfoControlView())
        );
    }

    // private void OnOpenFile(object sender, OpenFileMessage message)
    // {
    //     _selectedSideFileItemFullPath = message.FileItem.FullPath;
    //
    //     // 如果文件已经打开，则切换到已打开的标签页
    //     // 如果文件未打开，则打开新的标签页
    //     var openedTab = MainTabView.TabItems.FirstOrDefault(item =>
    //     {
    //         if (item is not TabViewItem tabViewItem)
    //         {
    //             return false;
    //         }
    //
    //         var view = tabViewItem.Content as IFileView;
    //         return view?.FullPath == message.FileItem.FullPath;
    //     });
    //
    //     if (openedTab is null)
    //     {
    //         // 打开新的标签页
    //         var content = GetContent(message.FileItem);
    //         var newTab = new TabViewItem { Content = content, Header = message.FileItem.Name };
    //         ToolTipService.SetToolTip(newTab, message.FileItem.FullPath);
    //         NavigateToNewTab(newTab);
    //
    //         _openedTabFileItems.Add(message.FileItem);
    //     }
    //     else
    //     {
    //         // 切换到已打开的标签页
    //         MainTabView.SelectedItem = openedTab;
    //     }
    // }

    private void MainTabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        var isRemoved = _openedTabFileItems.Remove(args.Item);
        Debug.Assert(isRemoved);
    }
}
