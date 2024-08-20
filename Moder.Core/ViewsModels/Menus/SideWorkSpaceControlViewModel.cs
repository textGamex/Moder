﻿using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Config;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class SideWorkSpaceControlViewModel : ObservableObject
{
	[ObservableProperty] private IEnumerable<SystemFileItem>? _items;

	public SideWorkSpaceControlViewModel(GlobalSettings globalSettings)
	{
		var items = new SystemFileItem(globalSettings.WorkRootFolderPath, false);
		LoadFileSystem(globalSettings.WorkRootFolderPath, items);
		Items = [items];
	}

	private void LoadFileSystem(string path, SystemFileItem parent)
	{
		var directories = Directory.GetDirectories(path);
		var files = Directory.GetFiles(path);

		foreach (var directoryPath in directories)
		{
			var item = new SystemFileItem(directoryPath, false);
			parent.Children.Add(item);
			LoadFileSystem(directoryPath, item);
		}

		foreach (var filePath in files)
		{
			parent.Children.Add(new SystemFileItem(filePath, true));
		}
	}
}