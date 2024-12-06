using CommunityToolkit.Mvvm.ComponentModel;
using Moder.Core.Models;
using Moder.Core.Services.Config;

namespace Moder.Core.ViewsModel.Menus;

public sealed class SideBarControlViewModel : ObservableObject
{
    public IReadOnlyList<SystemFileItem> Items => _root.Children;
    private readonly SystemFileItem _root;

    public SideBarControlViewModel(AppSettingService settingService)
    {
        _root = new SystemFileItem(settingService.ModRootFolderPath, false, null);
        LoadFileSystem(settingService.ModRootFolderPath, _root);
    }

    private static void LoadFileSystem(string path, SystemFileItem parent)
    {
        var directories = Directory.GetDirectories(path);
        var files = Directory.GetFiles(path);

        // TODO: Sort
        // Array.Sort(directories, WindowsStringComparer.Instance);
        // Array.Sort(files, WindowsStringComparer.Instance);

        foreach (var directoryPath in directories)
        {
            var item = new SystemFileItem(directoryPath, false, parent);
            parent.AddChild(item);
            LoadFileSystem(directoryPath, item);
        }

        foreach (var filePath in files)
        {
            parent.AddChild(filePath, true);
        }
    }
}
