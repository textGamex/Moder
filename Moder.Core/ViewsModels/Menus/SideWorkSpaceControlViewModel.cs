using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using MethodTimer;
using Microsoft.Extensions.Logging;
using Moder.Core.Services.Config;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class SideWorkSpaceControlViewModel : ObservableObject
{
    private readonly ILogger<SideWorkSpaceControlViewModel> _logger;

    // [ObservableProperty]
    public IReadOnlyList<SystemFileItem> Items => _root.Children;
    private readonly SystemFileItem _root;

    private readonly FileSystemWatcher _watcher;

    public SideWorkSpaceControlViewModel(
        GlobalSettingService globalSettings,
        ILogger<SideWorkSpaceControlViewModel> logger
    )
    {
        _logger = logger;

        _watcher = new FileSystemWatcher(globalSettings.ModRootFolderPath, "*.*");
        _watcher.Deleted += ContentOnDeleted;
        _watcher.Created += ContentOnCreated;
        _watcher.Renamed += ContentOnRenamed;
        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;

        var root = new SystemFileItem(globalSettings.ModRootFolderPath, false, null);
        LoadFileSystem(globalSettings.ModRootFolderPath, root);
        _root = root;
    }

    private void ContentOnRenamed(object sender, RenamedEventArgs e)
    {
        var target = FindFileItemByPath(e.OldFullPath, Items);
    }

    private void ContentOnDeleted(object sender, FileSystemEventArgs e)
    {
        var target = FindFileItemByPath(e.FullPath, Items);
        if (target is null)
        {
            _logger.LogWarning("找不到: {FullPath}", e.FullPath);
            return;
        }
        var parent = target.Parent;
        parent?.RemoveChild(target);
    }

    private void ContentOnCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Created: {FullPath}", e.FullPath);
        var directoryPath = Path.GetDirectoryName(e.FullPath);
        if (directoryPath is null)
        {
            return;
        }

        var isFile = !Directory.Exists(e.FullPath);
        var parent = FindFileItemByPath(directoryPath, Items);
        if (parent is null)
        {
            _logger.LogWarning("找不到插入的位置: {FullPath}", directoryPath);
            return;
        }

        var item = new SystemFileItem(e.FullPath, isFile, parent);
        var insertIndex = FindInsertIndex(parent.Children, item);
        parent.InsertChild(insertIndex, item);
        _logger.LogInformation("添加的位置: {FullPath}", parent?.FullPath ?? "null");
    }

    /// <summary>
    /// 查找系统文件夹或文件对应的 <see cref="SystemFileItem"/>
    /// </summary>
    /// <param name="fullPath">文件或文件夹的路径</param>
    /// <param name="items"></param>
    /// <returns></returns>
    private static SystemFileItem? FindFileItemByPath(string fullPath, IReadOnlyList<SystemFileItem> items)
    {
        for (var index = 0; index < items.Count; index++)
        {
            var fileItem = items[index];
            if (fileItem.FullPath == fullPath)
            {
                return fileItem;
            }

            var result = FindFileItemByPath(fullPath, fileItem.Children);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private static int FindInsertIndex(IReadOnlyList<SystemFileItem> parentChildren, SystemFileItem newItem)
    {
        if (parentChildren.Count == 0)
        {
            return 0;
        }

        //  如果是文件, 添加到最后一个文件之后
        var insertIndex = parentChildren.Count - 1;
        var maxIndex = parentChildren.Count;
        var lastFolderIndex = FindLastFolderIndex(parentChildren);
        var index = 0;

        // 当新增的是文件夹时，只与文件夹比较, 如果是文件，则只与文件比较
        if (newItem.IsFolder)
        {
            maxIndex = lastFolderIndex;
            // 未找到时，添加到最后一个文件夹之后,
            insertIndex = lastFolderIndex;
        }
        else
        {
            // 跳过所有文件夹
            index = lastFolderIndex + 1;
        }

        for (; index < maxIndex; index++)
        {
            if (WindowsStringComparer.Instance.Compare(newItem.FullPath, parentChildren[index].FullPath) == -1)
            {
                insertIndex = index;
                break;
            }
        }

        return insertIndex;
    }

    private static int FindLastFolderIndex(IReadOnlyList<SystemFileItem> items)
    {
        var i = 0;
        while (i < items.Count && items[i].IsFolder)
        {
            ++i;
        }

        return i == 0 ? 0 : i - 1;
    }

    private static void LoadFileSystem(string path, SystemFileItem parent)
    {
        var directories = Directory.GetDirectories(path);
        var files = Directory.GetFiles(path);

        Array.Sort(directories, WindowsStringComparer.Instance);
        Array.Sort(files, WindowsStringComparer.Instance);

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

    private sealed partial class WindowsStringComparer : IComparer<string>
    {
        public static WindowsStringComparer Instance { get; } = new();

        // CSharp 实现 https://www.codeproject.com/Articles/11016/Numeric-String-Sort-in-C?msg=1183262#xx1183262xx
        [LibraryImport("shlwapi.dll", SetLastError = false, StringMarshalling = StringMarshalling.Utf16)]
        private static partial int StrCmpLogicalW(string x, string y);

        public int Compare(string? x, string? y)
        {
            if (string.IsNullOrEmpty(x))
            {
                if (string.IsNullOrEmpty(y))
                {
                    return 0;
                }

                return -1;
            }
            if (string.IsNullOrEmpty(y))
            {
                return 1;
            }

            return StrCmpLogicalW(x, y);
        }
    }
}
