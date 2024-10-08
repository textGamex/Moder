using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Messages;
using Moder.Core.Models;
using Moder.Core.Services.Config;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class SettingsControlViewModel : ObservableObject
{
    // TODO: Tag 改为 Enum
    public ComboBoxItem[] ThemeMode { get; } =
        [
            new() { Content = "跟随系统设置", Tag = "Default" },
            new() { Content = "明亮", Tag = "Light" },
            new() { Content = "暗黑", Tag = "Dark" }
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
    private int _selectedThemeModeIndex;

    [ObservableProperty]
    private ComboBoxItem _selectedLanguage;

    private readonly GlobalSettingService _globalSettingService;

    public SettingsControlViewModel(GlobalSettingService globalSettingService)
    {
        _globalSettingService = globalSettingService;
        _selectedThemeModeIndex = GetSelectedThemeModeIndex();
        _selectedLanguage = GetSelectedLanguage();
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

    private int GetSelectedThemeModeIndex()
    {
        var themeMode = _globalSettingService.AppThemeMode;
        if (themeMode == ElementTheme.Default)
        {
            return 0;
        }

        if (themeMode == ElementTheme.Light)
        {
            return 1;
        }

        if (themeMode == ElementTheme.Dark)
        {
            return 2;
        }

        return 0;
    }

    partial void OnSelectedThemeModeIndexChanged(int value)
    {
        var tag = ThemeMode[value].Tag.ToString();
        if (tag == "Default")
        {
            SetThemeMode(ElementTheme.Default);
        }
        else if (tag == "Light")
        {
            SetThemeMode(ElementTheme.Light);
        }
        else if (tag == "Dark")
        {
            SetThemeMode(ElementTheme.Dark);
        }
        else
        {
            throw new ArgumentException("Invalid theme mode tag.");
        }
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
}
