using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Helper;
using Moder.Core.Messages;
using Moder.Core.Services.Config;
using Moder.Core.Views.Game;
using Moder.Core.Views.Menus;
using Moder.Core.ViewsModels;
using Moder.Core.ViewsModels.Menus;
using NLog;
using Windows.Foundation;
using Moder.Core.Models;

namespace Moder.Core.Views;

public sealed partial class MainWindow
{
    public MainWindowViewModel ViewModel { get; }

    private readonly GlobalSettingService _settings;
    private readonly IServiceProvider _serviceProvider;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 缓存已打开的文件标签页, 避免在侧边栏中查询
    /// </summary>
    private readonly List<SystemFileItem> _openedTabFileItems = new(8);

    private string _selectedSideFileItemFullPath = string.Empty;

    public MainWindow(
        MainWindowViewModel model,
        GlobalSettingService settings,
        IServiceProvider serviceProvider
    )
    {
        ViewModel = model;
        _settings = settings;
        _serviceProvider = serviceProvider;
        InitializeComponent();

        SetAppLanguage();
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/logo.ico"));
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;
        WindowHelper.SetSystemBackdropTypeByConfig(this);

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
            (_, _) =>
                SideContentControl.Content = _serviceProvider.GetRequiredService<SideWorkSpaceControlView>()
        );

        WeakReferenceMessenger.Default.Register<OpenFileMessage>(this, OnOpenFile);

        WeakReferenceMessenger.Default.Register<AppLanguageChangedMessage>(
            this,
            (_, _) =>
            {
                Bindings.Update();
            }
        );
    }

    private void SetAppLanguage()
    {
        if (_settings.AppLanguage != AppLanguageInfo.Default)
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(_settings.AppLanguage);
        }
    }

    private void OnOpenFile(object sender, OpenFileMessage message)
    {
        _selectedSideFileItemFullPath = message.FileItem.FullPath;

        // 如果文件已经打开，则切换到已打开的标签页
        // 如果文件未打开，则打开新的标签页
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
            var content = GetContent(message.FileItem);
            var newTab = new TabViewItem { Content = content, Header = message.FileItem.Name };
            ToolTipService.SetToolTip(newTab, message.FileItem.FullPath);
            NavigateToNewTab(newTab);

            _openedTabFileItems.Add(message.FileItem);
        }
        else
        {
            // 切换到已打开的标签页
            MainTabView.SelectedItem = openedTab;
        }
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
        var bounds = transform.TransformBounds(
            new Rect(
                0,
                0,
                SettingsButton.ActualWidth + CharacterEditorButton.ActualWidth,
                SettingsButton.ActualHeight + CharacterEditorButton.ActualHeight
            )
        );
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

    private void MainWindow_OnClosed(object sender, WindowEventArgs args)
    {
        _settings.SaveChanged();
    }

    private void MainTabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        var isRemoved = sender.TabItems.Remove(args.Tab);
        Debug.Assert(isRemoved);

        var tab = args.Tab.Content;
        // 关闭文件标签页时，从缓存列表中移除对应的文件并同步侧边栏选中项
        if (tab is IFileView fileView)
        {
            var index = _openedTabFileItems.FindIndex(item => item.FullPath == fileView.FullPath);
            if (index == -1)
            {
                Log.Warn("未在标签文件缓存列表中找到指定文件, Path: {Path}", fileView.FullPath);
            }
            else
            {
                _openedTabFileItems.RemoveAt(index);
                if (sender.TabItems.Count == 0)
                {
                    WeakReferenceMessenger.Default.Send(new SyncSideWorkSelectedItemMessage(null));
                }
            }
        }
        else if (tab is SettingsControlView settings)
        {
            settings.SaveChanged();
        }

        if (tab is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private void MainTabView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabView.SelectedItem is not TabViewItem currentTab)
        {
            return;
        }

        // 如果切换到的标签页不是文件标签页 (比如设置标签页)，则清空侧边栏选中项
        if (currentTab.Content is not IFileView currentFileView)
        {
            ClearSideWorkSelectState();
            return;
        }

        // 切换标签页时，同步侧边栏选中项
        if (_selectedSideFileItemFullPath != currentFileView.FullPath)
        {
            _selectedSideFileItemFullPath = currentFileView.FullPath;
            var target = _openedTabFileItems.Find(item => item.FullPath == currentFileView.FullPath);
            Debug.Assert(target is not null, "在标签文件缓存列表中未找到目标文件");
            WeakReferenceMessenger.Default.Send(new SyncSideWorkSelectedItemMessage(target));
        }
    }

    private void ClearSideWorkSelectState()
    {
        WeakReferenceMessenger.Default.Send(new SyncSideWorkSelectedItemMessage(null));
        _selectedSideFileItemFullPath = string.Empty;
    }

    /// <summary>
    /// 打开设置标签页
    /// </summary>
    private void TitleBarSettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var isSelected = MainTabView.SelectedItem is TabViewItem { Content: SettingsControlView };
        if (isSelected)
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
        var settingsTab = new TabViewItem
        {
            Content = settingsView,
            Header = Language.Strings.Resource.Menu_Settings,
            IconSource = new FontIconSource { Glyph = "\uE713" }
        };
        NavigateToNewTab(settingsTab);
    }

    private void CharacterEditorButton_OnClick(object sender, RoutedEventArgs e)
    {
        var isSelected = MainTabView.SelectedItem is TabViewItem { Content: CharacterEditorControlView };
        if (isSelected)
        {
            return;
        }

        var existingTab = MainTabView
            .TabItems.Cast<TabViewItem>()
            .FirstOrDefault(item => item.Content is CharacterEditorControlView);

        if (existingTab is not null)
        {
            MainTabView.SelectedItem = existingTab;
            return;
        }

        var editorView = new CharacterEditorControlView();
        var editorTab = new TabViewItem
        {
            Content = editorView,
            Header = Language.Strings.Resource.Menu_CharacterEditor,
            IconSource = new FontIconSource { Glyph = "\uE70F" }
        };
        NavigateToNewTab(editorTab);
    }

    /// <summary>
    /// 添加标签页并切换到新标签页, 在此方法运行之后，会触发 <see cref="MainTabView_OnSelectionChanged"/> 方法
    /// </summary>
    /// <param name="tab">添加的标签页</param>
    private void NavigateToNewTab(TabViewItem tab)
    {
        MainTabView.TabItems.Add(tab);
        MainTabView.SelectedItem = tab;
    }
}
