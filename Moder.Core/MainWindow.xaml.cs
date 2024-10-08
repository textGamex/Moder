using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Moder.Core.Messages;
using Moder.Core.Services.Config;
using Moder.Core.Views;
using Moder.Core.Views.Game;
using Moder.Core.Views.Menus;
using Moder.Core.ViewsModels.Menus;
using Windows.Foundation;
using WinUIEx;

namespace Moder.Core;

public sealed partial class MainWindow
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
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;
        SystemBackdrop = new MicaBackdrop { Kind = MicaKind.Base };
        ViewModel = model;
        AppTitleBar.Loaded += AppTitleBarOnLoaded;
        AppTitleBar.SizeChanged += AppTitleBarOnSizeChanged;

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

    private void AppTitleBarOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        SetRegionsForCustomTitleBar();
    }

    private void AppTitleBarOnLoaded(object sender, RoutedEventArgs e)
    {
        SetRegionsForCustomTitleBar();
    }

    private void SetRegionsForCustomTitleBar()
    {
        var scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;
        RightPaddingColumn.Width = new GridLength(AppWindow.TitleBar.RightInset / scaleAdjustment);
        LeftPaddingColumn.Width = new GridLength(AppWindow.TitleBar.LeftInset / scaleAdjustment);
        var transform = SettingsButton.TransformToVisual(null);
        var bounds = transform.TransformBounds(new Rect(0, 0, SettingsButton.ActualWidth, SettingsButton.ActualHeight));
        var settingsButtonRect = GetRect(bounds, scaleAdjustment);

        var rectArray = new[] { settingsButtonRect };

        var nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
        nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);
    }

    private static Windows.Graphics.RectInt32 GetRect(Rect bounds, double scale)
    {
        return new Windows.Graphics.RectInt32(
            _X: (int)Math.Round(bounds.X * scale),
            _Y: (int)Math.Round(bounds.Y * scale),
            _Width: (int)Math.Round(bounds.Width * scale),
            _Height: (int)Math.Round(bounds.Height * scale)
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

        if (args.Tab.Content is IFileView fileView)
        {
            _openedTabFileItems.RemoveAt(_openedTabFileItems.FindIndex(item => item.FullPath == fileView.FullPath));

            if (sender.TabItems.Count == 0)
            {
                WeakReferenceMessenger.Default.Send(new SyncSideWorkSelectedItemMessage(null));
            }
        }
    }

    private void MainTabView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabView.SelectedItem is not TabViewItem currentTab)
        {
            return;
        }

        if (currentTab.Content is not IFileView currentFileView)
        {
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

    private void TitleBarSettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (MainTabView.SelectedItem is TabViewItem { Content: SettingsControlView })
        {
            return;
        }

        var existingSettingsTab = MainTabView
            .TabItems.Cast<TabViewItem>()
            .FirstOrDefault(item => item.Content is SettingsControlView);

        if (existingSettingsTab is not null)
        {
            MainTabView.SelectedItem = existingSettingsTab;
            return;
        }

        var settingsView = _serviceProvider.GetRequiredService<SettingsControlView>();
        var settingsTab = new TabViewItem { Content = settingsView, Header = "设置" };
        MainTabView.TabItems.Add(settingsTab);
        MainTabView.SelectedItem = settingsTab;
    }
}
