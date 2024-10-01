using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Moder.Core.Messages;
using Moder.Core.Services.Config;
using Moder.Core.Views;
using Moder.Core.Views.Game;
using Moder.Core.Views.Menus;
using Moder.Core.ViewsModels.Menus;

namespace Moder.Core;

public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    private readonly GlobalSettingService _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainWindow> _logger;
    private readonly List<SystemFileItem> _openedTabFileItems = new(16);
    private SystemFileItem? _latestFileItem;

    public MainWindow(
        MainWindowViewModel model,
        GlobalSettingService settings,
        IServiceProvider serviceProvider,
        ILogger<MainWindow> logger
    )
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
        _logger = logger;
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
        ViewModel = model;

        if (string.IsNullOrEmpty(settings.ModRootFolderPath))
        {
            SideContentControl.Content = _serviceProvider.GetRequiredService<OpenFolderControlView>();
        }
        else
        {
            SideContentControl.Content = _serviceProvider.GetRequiredService<SideWorkSpaceControlView>();
        }

        WeakReferenceMessenger.Default.Register<CompleteWorkFolderSelectMessage>(
            this,
            (_, _) => SideContentControl.Content = _serviceProvider.GetRequiredService<SideWorkSpaceControlView>()
        );
        WeakReferenceMessenger.Default.Register<OpenFileMessage>(
            this,
            (_, message) =>
            {
                _latestFileItem = message.FileItem;

                var content = GetContent(message.FileItem);
                var openedTab = MainTabView.TabItems.FirstOrDefault(item =>
                {
                    if (item is not TabViewItem tabViewItem)
                    {
                        return false;
                    }

                    var view = tabViewItem.Content as IFileView;
                    return view?.FullPath == message.FileItem.FullPath;
                });

                if (openedTab is null)
                {
                    // 打开新的标签页
                    var newTab = new TabViewItem { Content = content, Header = message.FileItem.Name };
                    ToolTipService.SetToolTip(newTab, message.FileItem.FullPath);
                    MainTabView.TabItems.Add(newTab);
                    MainTabView.SelectedItem = newTab;

                    _openedTabFileItems.Add(message.FileItem);
                }
                else
                {
                    // 切换到已打开的标签页
                    MainTabView.SelectedItem = openedTab;
                }
            }
        );
    }

    private IFileView GetContent(SystemFileItem fileItem)
    {
        var relativePath = Path.GetRelativePath(_settings.ModRootFolderPath, fileItem.FullPath);
        if (relativePath.Contains("states"))
        {
            return _serviceProvider.GetRequiredService<StateFileControlView>();
        }

        return _serviceProvider.GetRequiredService<NotSupportInfoControlView>();
    }

    private void MainWindow_OnClosed(object sender, WindowEventArgs args)
    {
        _logger.LogInformation("配置文件保存中...");
        _settings.Save();
        _logger.LogInformation("配置文件保存完成");
    }

    private void MainTabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        sender.TabItems.Remove(args.Tab);

        var fileView = (IFileView)args.Tab.Content;
        _openedTabFileItems.RemoveAt(_openedTabFileItems.FindIndex(item => item.FullPath == fileView.FullPath));

        if (sender.TabItems.Count == 0)
        {
            WeakReferenceMessenger.Default.Send(new SyncSideWorkSelectedItemMessage(null));
        }
    }

    private void MainTabView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var currentTab = MainTabView.SelectedItem as TabViewItem;
        if (currentTab?.Content is not IFileView currentFileView)
        {
            _logger.LogWarning("Tab内容为空");
            return;
        }

        Debug.Assert(_latestFileItem is not null, "当前文件为空");
        if (_latestFileItem?.FullPath != currentFileView.FullPath)
        {
            var target = _openedTabFileItems.Find(item => item.FullPath == currentFileView.FullPath);
            Debug.Assert(target is not null, "在标签文件缓存列表中未找到目标文件");
            WeakReferenceMessenger.Default.Send(new SyncSideWorkSelectedItemMessage(target));
        }
    }
}
