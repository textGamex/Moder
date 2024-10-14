using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage;
using Windows.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class SystemFileItem
{
    public string Name { get; }
    public string FullPath { get; }
    public bool IsFile { get; }
    public ObservableCollection<SystemFileItem> Children { get; } = [];

    private static readonly ILogger<SystemFileItem> Logger =
        App.Current.Services.GetRequiredService<ILogger<SystemFileItem>>();

    public SystemFileItem(string fullPath, bool isFile)
    {
        Name = Path.GetFileName(fullPath);
        FullPath = fullPath;
        IsFile = isFile;
    }

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(FullPath)}: {FullPath}, {nameof(IsFile)}: {IsFile}, {nameof(Children)}: {Children}";
    }

    [RelayCommand]
    private async Task ShowInExplorer()
    {
        string? folder;
        IStorageItem selectedItem;
        if (IsFile)
        {
            selectedItem = await StorageFile.GetFileFromPathAsync(FullPath);
            folder = Path.GetDirectoryName(FullPath);
        }
        else
        {
            selectedItem = await StorageFolder.GetFolderFromPathAsync(FullPath);
            folder = Directory.GetParent(FullPath)?.FullName;
        }

        if (folder is null)
        {
            Logger.LogWarning("在资源管理器中打开失败，无法获取路径：{FullPath}", FullPath);
            return;
        }
        await Launcher.LaunchFolderPathAsync(
            folder,
            new FolderLauncherOptions { ItemsToSelect = { selectedItem } }
        );
    }
}
