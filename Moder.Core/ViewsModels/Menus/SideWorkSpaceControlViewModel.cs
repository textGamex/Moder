using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Config;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class SideWorkSpaceControlViewModel : ObservableObject
{
	[ObservableProperty]
	private IEnumerable<SystemFileItem>? _items;

	public SideWorkSpaceControlViewModel(GlobalSettings globalSettings)
	{
		var items = new SystemFileItem() { Name = Path.GetFileName(globalSettings.WorkRootFolderPath), IsFile = false };
		LoadFileSystem(globalSettings.WorkRootFolderPath, items);
		Items = [items];
	}

	private void LoadFileSystem(string path, SystemFileItem parent)
	{
		var directories = Directory.GetDirectories(path);
		var files = Directory.GetFiles(path);

		foreach (var directory in directories)
		{
			var item = new SystemFileItem { Name = Path.GetFileName(directory), IsFile = false };
			parent.Children.Add(item);
			LoadFileSystem(directory, item);
		}

		foreach (var file in files)
		{
			parent.Children.Add(new SystemFileItem { Name = Path.GetFileName(file), IsFile = true });
		}
	}
}