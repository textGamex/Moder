using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Messages;
using Moder.Core.Models;
using Moder.Core.Services.Config;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WinUIEx;

namespace Moder.Core.ViewsModels.Menus;

/*----------------------------------------------------------------
 * 描述：设置页面的前端操作入口
 * ----------------------------------------------------------------
 * 修改人：ChiangHeng
 * 时间：2024/10/10
 * 修改说明：确认保存之后执行修改
 *----------------------------------------------------------------*/
/// <summary>
/// 设置页面的前端入口
/// </summary>
public sealed partial class SettingsControlViewModel : ObservableObject
{
    #region 临时存储变量等待确认同意提交

    private GameLanguage? _tempGameLanguage = null;
    private ElementTheme? _tempElementTheme = null;
    private string _tempGameRootPath = string.Empty;
    private string _tempModRootPath = string.Empty;

    #endregion

    public string GameRootPath => _globalSettingService.GameRootFolderPath;
    public string ModRootPath => _globalSettingService.ModRootFolderPath;

    public ComboBoxItem[] ThemeMode { get; } =
        [
            new() { Content = "跟随系统设置", Tag = ElementTheme.Default },
            new() { Content = "明亮", Tag = ElementTheme.Light },
            new() { Content = "暗黑", Tag = ElementTheme.Dark }
        ];

    public ComboBoxItem[] Languages { get; } =
        [
            new() { Content = "跟随系统设置", Tag = GameLanguage.Default },
            new() { Content = "简体中文", Tag = GameLanguage.Chinese },
            new() { Content = "英文", Tag = GameLanguage.English },
            new() { Content = "日语", Tag = GameLanguage.Japanese },
            new() { Content = "俄语", Tag = GameLanguage.Russian },
            new() { Content = "法语", Tag = GameLanguage.French },
            new() { Content = "波兰语", Tag = GameLanguage.Polish },
            new() { Content = "德语", Tag = GameLanguage.German },
            new() { Content = "西班牙语", Tag = GameLanguage.Spanish },
            new() { Content = "巴西葡萄牙语", Tag = GameLanguage.Portuguese }
        ];

    [ObservableProperty]
    private ComboBoxItem _selectedThemeMode;

    [ObservableProperty]
    private ComboBoxItem _selectedLanguage;

    private readonly GlobalSettingService _globalSettingService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    /// <param name="globalSettingService"> 设置业务 </param>
    public SettingsControlViewModel(GlobalSettingService globalSettingService)
    {
        _globalSettingService = globalSettingService;
        _selectedLanguage = GetSelectedLanguage();
        _selectedThemeMode = GetSelectedThemeMode();
    }

    /// <summary>
    /// <para> 返回选择的语言 </para>
    /// <para> 如果存储的值不在字典内默认返回 [跟随系统设置] </para>
    /// <para> 但是实际上并不修改存储的值 </para>
    /// </summary>
    /// <returns> 选择的语言类型 </returns>
    private ComboBoxItem GetSelectedLanguage()
    {
        var language = _globalSettingService.GameLanguage;
        foreach (var item in Languages)
        {
            if ((GameLanguage)item.Tag == language)
            {
                return item;
            }
        }
        return Languages[0];
    }

    /// <summary>
    /// <para> 获取主题设置 </para>
    /// <para> 如果存储的值不在字典内默认返回[跟随系统设置] </para>
    /// <para> 但是实际上并不修改存储的值 </para>
    /// </summary>
    /// <returns> 返回值提供前展示 </returns>
    private ComboBoxItem GetSelectedThemeMode()
    {
        var themeMode = _globalSettingService.AppThemeMode;
        foreach (var item in ThemeMode)
        {
            if ((ElementTheme)item.Tag == themeMode)
            {
                return item;
            }
        }
        return ThemeMode[0];
    }

    /// <summary>
    /// 修改主题的值到临时变量池中
    /// </summary>
    /// <param name="value"> 前端提交需要被修改的值 </param>
    partial void OnSelectedThemeModeChanged(ComboBoxItem value)
    {
        var theme = (ElementTheme)value.Tag;
        _tempElementTheme = theme;
        //SetThemeMode(theme);
    }

    /// <summary>
    /// 修改主题的值
    /// <para> 曾经被 <see cref="OnSelectedThemeModeChanged"/>调用</para>
    /// </summary>
    /// <param name="theme"></param>
    [Obsolete]
    private void SetThemeMode(ElementTheme theme)
    {
        var window = App.Current.MainWindow;
        if (window.Content is FrameworkElement root)
        {
            root.RequestedTheme = theme;
        }

        _globalSettingService.AppThemeMode = theme;
    }

    /// <summary>
    /// 修改语言的值
    /// </summary>
    /// <param name="value"> 前端提交需要被修改的值 </param>
    partial void OnSelectedLanguageChanged(ComboBoxItem value)
    {
        _tempGameLanguage = (GameLanguage)value.Tag;
    }

    /// <summary>
    /// 修改游戏主目录并且读取
    /// <para><c> 修改并且重新解析 </c></para>
    /// <para>通过内部方法打开路径选择框<see cref="ShowOpenFolderDialogAsync"/></para>
    /// </summary>
    /// <returns> 创建线程异步执行 </returns>
    [RelayCommand]
    private async Task SelectGameRootPathAsync()
    {
        var folder = await ShowOpenFolderDialogAsync();
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        _tempGameRootPath = folder;
        OnPropertyChanged(nameof(GameRootPath));
    }

    /// <summary>
    /// 修改Mod目录
    /// <para><c> 修改并且重新解析 </c></para>
    /// <para>通过内部方法打开路径选择框<see cref="ShowOpenFolderDialogAsync"/></para>
    /// </summary>
    /// <returns> 创建线程异步执行 </returns>
    [RelayCommand]
    private async Task SelectModRootPathAsync()
    {
        var folder = await ShowOpenFolderDialogAsync();
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        _tempModRootPath = folder;
        OnPropertyChanged(nameof(ModRootPath));
    }

    /// <summary>
    /// 打开文件目录选择框
    /// </summary>
    /// <returns> 通过非空校验的路径 </returns>
    private static async Task<string> ShowOpenFolderDialogAsync()
    {
        var dialog = new FolderPicker();
        InitializeWithWindow.Initialize(dialog, App.Current.MainWindow.GetWindowHandle());
        var folder = await dialog.PickSingleFolderAsync();

        return folder is null ? string.Empty : folder.Path;
    }

    /// <summary>
    /// 保存修改
    /// </summary>
    [RelayCommand]
    private async void SaveUpdate()
    {
        // 修改APP主题
        var window = App.Current.MainWindow;

        if (_tempElementTheme != null)
        {
            if (window.Content is FrameworkElement root)
            {
                root.RequestedTheme = (ElementTheme)_tempElementTheme;
            }
            _globalSettingService.AppThemeMode = (ElementTheme)_tempElementTheme;
        }

        // 修改游戏语言
        if (_tempGameLanguage != null)
        {
            _globalSettingService.GameLanguage = (GameLanguage)_tempGameLanguage;
            WeakReferenceMessenger.Default.Send(new ReloadLocalizationFiles());
        }

        // 修改游戏路径
        if (!string.IsNullOrEmpty(_tempGameRootPath))
        {
            _globalSettingService.GameRootFolderPath = _tempGameRootPath;
            OnPropertyChanged(nameof(GameRootPath));
        }

        // 修改mod路径
        if (!string.IsNullOrEmpty(_tempModRootPath))
        {
            _globalSettingService.ModRootFolderPath = _tempModRootPath;
            OnPropertyChanged(nameof(ModRootPath));
        }
    }
}
