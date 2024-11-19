using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Helper;
using Moder.Core.Messages;
using Moder.Core.Models;
using Moder.Core.Models.Vo;
using Moder.Core.Services.Config;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WinUIEx;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class SettingsControlViewModel : ObservableObject
{
    public string GameRootPath => _globalSettingService.GameRootFolderPath;
    public string ModRootPath => _globalSettingService.ModRootFolderPath;

    public ComboBoxItem[] ThemeMode { get; } =
        [
            new() { Content = "跟随系统设置", Tag = ElementTheme.Default },
            new() { Content = "明亮", Tag = ElementTheme.Light },
            new() { Content = "暗黑", Tag = ElementTheme.Dark }
        ];

    public BackdropTypeItemVo[] BackdropTypes { get; } = GetBackdropType();

    /// <summary>
    /// 获取所有支持的背景类型
    /// </summary>
    /// <returns>所有支持的背景类型</returns>
    private static BackdropTypeItemVo[] GetBackdropType()
    {
        var list = new List<BackdropTypeItemVo>(5)
        {
            new("默认", WindowBackdropType.Default),
            new("无背景", WindowBackdropType.None)
        };

        if (MicaController.IsSupported())
        {
            list.Add(new BackdropTypeItemVo("云母", WindowBackdropType.Mica));
            list.Add(new BackdropTypeItemVo("云母 Alt", WindowBackdropType.MicaAlt));
        }

        if (DesktopAcrylicController.IsSupported())
        {
            list.Add(new BackdropTypeItemVo("亚克力", WindowBackdropType.Acrylic));
        }

        return list.ToArray();
    }

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
    private BackdropTypeItemVo _selectedBackdropType;

    [ObservableProperty]
    private ComboBoxItem _selectedLanguage;

    private readonly GlobalSettingService _globalSettingService;

    public SettingsControlViewModel(GlobalSettingService globalSettingService)
    {
        _globalSettingService = globalSettingService;
        _selectedLanguage = GetSelectedLanguage();
        _selectedThemeMode = GetSelectedThemeMode();
        _selectedBackdropType = GetSelectedBackdropType();
    }

    private BackdropTypeItemVo GetSelectedBackdropType()
    {
        var backdrop = _globalSettingService.WindowBackdropType;
        return BackdropTypes.First(backdropTypeItem => backdropTypeItem.Backdrop == backdrop);
    }

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

    partial void OnSelectedThemeModeChanged(ComboBoxItem value)
    {
        var theme = (ElementTheme)value.Tag;
        SetThemeMode(theme);
    }

    private void SetThemeMode(ElementTheme theme)
    {
        var window = App.Current.MainWindow;
        if (window.Content is FrameworkElement root)
        {
            root.RequestedTheme = theme;
        }
        _globalSettingService.AppThemeMode = theme;
    }

    partial void OnSelectedLanguageChanged(ComboBoxItem value)
    {
        var language = (GameLanguage)value.Tag;
        _globalSettingService.GameLanguage = language;

        WeakReferenceMessenger.Default.Send(new ReloadLocalizationFiles());
    }

    partial void OnSelectedBackdropTypeChanged(BackdropTypeItemVo value)
    {
        _globalSettingService.WindowBackdropType = value.Backdrop;
        WindowHelper.SetSystemBackdropTypeByConfig();
    }

    [RelayCommand]
    private async Task SelectGameRootPathAsync()
    {
        var folder = await ShowOpenFolderDialogAsync();
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        _globalSettingService.GameRootFolderPath = folder;
        OnPropertyChanged(nameof(GameRootPath));
    }

    [RelayCommand]
    private async Task SelectModRootPathAsync()
    {
        var folder = await ShowOpenFolderDialogAsync();
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        _globalSettingService.ModRootFolderPath = folder;
        OnPropertyChanged(nameof(ModRootPath));
    }

    private static async Task<string> ShowOpenFolderDialogAsync()
    {
        var dialog = new FolderPicker();
        InitializeWithWindow.Initialize(dialog, App.Current.MainWindow.GetWindowHandle());
        var folder = await dialog.PickSingleFolderAsync();

        return folder is null ? string.Empty : folder.Path;
    }
}
