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
}