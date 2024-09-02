using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.Messaging;
using Moder.Core.Messages;
using WinUIEx;
using Moder.Core.Services.Config;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class OpenFolderControlViewModel : ObservableObject
{
	private readonly GlobalSettingService _globalSettings;

	public OpenFolderControlViewModel(GlobalSettingService globalSettings)
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
			_globalSettings.ModRootFolderPath = result.Path;
			WeakReferenceMessenger.Default.Send(new CompleteWorkFolderSelectMessage());
		}
	}
}