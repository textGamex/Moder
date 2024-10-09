using Windows.System;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Moder.Core.ViewsModels.Menus;

namespace Moder.Core.Views.Menus;

public sealed partial class SettingsControlView
{
	public SettingsControlViewModel ViewModel => (SettingsControlViewModel)DataContext;
	public SettingsControlView(SettingsControlViewModel settingsViewModel)
	{
		InitializeComponent();

		DataContext = settingsViewModel;
	}

	private async void OnRootPathCardClicked(object sender, RoutedEventArgs e)
	{
		var card = (SettingsCard)sender;
		await Launcher.LaunchFolderPathAsync(card.Description.ToString());
	}
}