using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Services.Config;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class SettingsControlViewModel : ObservableObject
{
    public static ComboBoxItem[] ThemeMode =>
        [
            new() { Content = "跟随系统设置", Tag = "Default" },
            new() { Content = "明亮", Tag = "Light" },
            new() { Content = "暗黑", Tag = "Dark" }
        ];

    [ObservableProperty]
    private int _selectedThemeModeIndex;

    private readonly GlobalSettingService _globalSettingService;

    public SettingsControlViewModel(GlobalSettingService globalSettingService)
    {
        _globalSettingService = globalSettingService;
        _selectedThemeModeIndex = GetSelectedThemeModeIndex();
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
}
