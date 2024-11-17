using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Moder.Core.Services.Config;
using Moder.Core.ViewsModels.Menus;
using Windows.System;

namespace Moder.Core.Views.Menus;

public sealed partial class SettingsControlView
{
    public static string RuntimeInfo => $"Runtime: .NET {Environment.Version.ToString()}";

    public SettingsControlViewModel ViewModel => (SettingsControlViewModel)DataContext;
    private readonly GlobalSettingService _globalSettingService;

    public SettingsControlView(
        SettingsControlViewModel settingsViewModel,
        GlobalSettingService globalSettingService
    )
    {
        _globalSettingService = globalSettingService;
        InitializeComponent();

        DataContext = settingsViewModel;
    }

    private async void OnRootPathCardClicked(object sender, RoutedEventArgs e)
    {
        var card = (SettingsCard)sender;
        await Launcher.LaunchFolderPathAsync(card.Description.ToString());
    }

    private async void OnGitHubUrlCardClicked(object sender, RoutedEventArgs e)
    {
        var card = (SettingsCard)sender;
        await Launcher.LaunchUriAsync(
            new Uri(card.Description.ToString() ?? throw new InvalidOperationException())
        );
    }

    /// <summary>
    /// 如果有更改，保存更改
    /// </summary>
    public void SaveChanged()
    {
        _globalSettingService.SaveChanged();
    }
}
