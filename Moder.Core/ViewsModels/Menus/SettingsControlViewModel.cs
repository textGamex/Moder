using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Moder.Core.Helper;
using Moder.Core.Messages;
using Moder.Core.Models;
using Moder.Core.Models.Vo;
using Moder.Core.Services.Config;
using Moder.Language.Strings;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class SettingsControlViewModel : ObservableObject
{
    public string GameRootPath => _globalSettingService.GameRootFolderPath;
    public string ModRootPath => _globalSettingService.ModRootFolderPath;

    [ObservableProperty]
    private ThemeModeInfo[] _themeMode;

    [ObservableProperty]
    private BackdropTypeItemVo[] _backdropTypes;

    public GameLanguageInfo[] GameLanguages { get; } =
        [
            new("跟随系统设置", GameLanguage.Default),
            new("简体中文", GameLanguage.Chinese),
            new("英文", GameLanguage.English),
            new("日语", GameLanguage.Japanese),
            new("俄语", GameLanguage.Russian),
            new("法语", GameLanguage.French),
            new("波兰语", GameLanguage.Polish),
            new("德语", GameLanguage.German),
            new("西班牙语", GameLanguage.Spanish),
            new("巴西葡萄牙语", GameLanguage.Portuguese)
        ];

    public AppLanguageInfo[] ApplicationLanguages { get; } =
        [new("跟随系统设置", string.Empty), new("简体中文", "zh-CN"), new("English", "en-US")];

    [ObservableProperty]
    private AppLanguageInfo _selectedAppLanguage;

    [ObservableProperty]
    private int _selectedThemeModeIndex;

    [ObservableProperty]
    private int _selectedBackdropTypeIndex;

    [ObservableProperty]
    private int _selectedGameLanguageIndex;

    private readonly GlobalSettingService _globalSettingService;

    public SettingsControlViewModel(GlobalSettingService globalSettingService)
    {
        _globalSettingService = globalSettingService;

        _themeMode = GetThemeMode();
        _selectedThemeModeIndex = GetSelectedThemeModeIndex();
        _backdropTypes = GetBackdropType();
        _selectedBackdropTypeIndex = GetSelectedBackdropTypeIndex();
        _selectedGameLanguageIndex = GetSelectedGameLanguageIndex();
        _selectedAppLanguage = GetSelectedAppLanguage();
    }

    private static ThemeModeInfo[] GetThemeMode()
    {
        return
        [
            new ThemeModeInfo(Resource.Common_UseSystemSetting, ElementTheme.Default),
            new ThemeModeInfo(Resource.ThemeMode_Light, ElementTheme.Light),
            new ThemeModeInfo(Resource.ThemeMode_Dark, ElementTheme.Dark)
        ];
    }

    /// <summary>
    /// 获取所有支持的背景类型
    /// </summary>
    /// <returns>所有支持的背景类型</returns>
    private static BackdropTypeItemVo[] GetBackdropType()
    {
        var list = new List<BackdropTypeItemVo>(5)
        {
            new(Resource.Common_Default, WindowBackdropType.Default),
            new(Resource.Backdrop_None, WindowBackdropType.None)
        };

        if (MicaController.IsSupported())
        {
            list.Add(new BackdropTypeItemVo(Resource.Backdrop_Mica, WindowBackdropType.Mica));
            list.Add(new BackdropTypeItemVo(Resource.Backdrop_MicaAlt, WindowBackdropType.MicaAlt));
        }

        if (DesktopAcrylicController.IsSupported())
        {
            list.Add(new BackdropTypeItemVo(Resource.Backdrop_Acrylic, WindowBackdropType.Acrylic));
        }

        return list.ToArray();
    }

    private int GetSelectedBackdropTypeIndex()
    {
        var backdrop = _globalSettingService.WindowBackdropType;
        return Array.FindIndex(BackdropTypes, item => item.Backdrop == backdrop);
    }

    private int GetSelectedGameLanguageIndex()
    {
        var language = _globalSettingService.GameLanguage;
        var index = Array.FindIndex(GameLanguages, item => item.Type == language);
        return index == -1 ? 0 : index;
    }

    private int GetSelectedThemeModeIndex()
    {
        var themeMode = _globalSettingService.AppThemeMode;
        var index = Array.FindIndex(ThemeMode, item => item.Mode == themeMode);
        return index == -1 ? 0 : index;
    }

    private AppLanguageInfo GetSelectedAppLanguage()
    {
        var code = _globalSettingService.AppLanguage;
        return Array.Find(ApplicationLanguages, language => language.Code == code) ?? ApplicationLanguages[0];
    }

    partial void OnSelectedThemeModeIndexChanged(int value)
    {
        // 当重新加载 ThemeMode 时, 会传入-1
        if (value == -1)
        {
            return;
        }
        SetThemeMode(ThemeMode[value].Mode);
    }

    private void SetThemeMode(ElementTheme theme)
    {
        WindowHelper.SetAppTheme(theme);
        _globalSettingService.AppThemeMode = theme;
    }

    partial void OnSelectedGameLanguageIndexChanged(int value)
    {
        // 当重新加载 GameLanguages 时, 会传入-1
        if (value == -1)
        {
            return;
        }

        var language = GameLanguages[value].Type;
        _globalSettingService.GameLanguage = language;

        // TODO: 重新实现热重载游戏本地化语言
        // WeakReferenceMessenger.Default.Send(new ReloadLocalizationFiles());
    }

    partial void OnSelectedBackdropTypeIndexChanged(int value)
    {
        // 当重新加载 BackdropTypes 时, 会传入-1
        if (value == -1)
        {
            return;
        }

        var backdrop = BackdropTypes[value].Backdrop;
        if (backdrop == _globalSettingService.WindowBackdropType)
        {
            return;
        }

        _globalSettingService.WindowBackdropType = backdrop;
        WindowHelper.SetSystemBackdropTypeByConfig();
    }

    partial void OnSelectedAppLanguageChanged(AppLanguageInfo value)
    {
        CultureInfo.CurrentUICulture =
            value.Code == AppLanguageInfo.Default
                ? CultureInfo.InstalledUICulture
                : CultureInfo.GetCultureInfo(value.Code);

        _globalSettingService.AppLanguage = value.Code;
        OnAppLanguageChanged();

        WeakReferenceMessenger.Default.Send(new AppLanguageChangedMessage());
    }

    private void OnAppLanguageChanged()
    {
        ThemeMode = GetThemeMode();
        SelectedThemeModeIndex = GetSelectedThemeModeIndex();
        BackdropTypes = GetBackdropType();
        SelectedBackdropTypeIndex = GetSelectedBackdropTypeIndex();
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
        var dialog = WindowHelper.CreateFolderPicker();
        var folder = await dialog.PickSingleFolderAsync();

        return folder is null ? string.Empty : folder.Path;
    }
}
