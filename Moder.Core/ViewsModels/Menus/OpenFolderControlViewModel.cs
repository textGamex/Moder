using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Moder.Core.Config;
using Windows.Storage.Pickers;
using WinUIEx;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class OpenFolderControlViewModel : ObservableObject
{
	private readonly GlobalSettings _globalSettings;

	public OpenFolderControlViewModel(GlobalSettings globalSettings)
	{
		_globalSettings = globalSettings;
	}

	[RelayCommand]
	private async Task OpenFolderAsync()
	{
		var folderPicker = new FolderPicker();
		WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.Current.MainWindow.GetWindowHandle());
		var result = await folderPicker.PickSingleFolderAsync();
		if (result is null)
		{
			return;
		}
		else
		{
			_globalSettings.WorkRootFolderPath = result.Path;

		}
	}
}