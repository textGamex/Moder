﻿using CommunityToolkit.Mvvm.ComponentModel;
using Moder.Core.Infrastructure;
using Moder.Core.Infrastructure.FileSort;
using Moder.Core.Models;
using Moder.Core.Services.Config;
using NLog;

namespace Moder.Core.ViewsModel.Menus;

public sealed class SideBarControlViewModel : ObservableObject
{
    public IReadOnlyList<SystemFileItem> Items => _root.Children;
    private readonly SystemFileItem _root;
    private readonly FileSystemSafeWatcher _fileWatcher;
    private readonly IFileSortComparer _fileSortComparer;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public SideBarControlViewModel(AppSettingService settingService, IFileSortComparer fileSortComparer)
    {
        _fileSortComparer = fileSortComparer;

        _fileWatcher = new FileSystemSafeWatcher(settingService.ModRootFolderPath, "*.*");
        _fileWatcher.Deleted += ContentOnDeleted;
        _fileWatcher.Created += ContentOnCreated;
        _fileWatcher.Renamed += ContentOnRenamed;
        _fileWatcher.Changed += ContentOnChanged;
        _fileWatcher.IncludeSubdirectories = true;
        _fileWatcher.EnableRaisingEvents = true;

        _root = new SystemFileItem(settingService.ModRootFolderPath, false, null);
        LoadFileSystem(settingService.ModRootFolderPath, _root);
    }

    private void ContentOnChanged(object sender, FileSystemEventArgs e)
    {
        Log.Trace("Changed: {FullPath}", e.FullPath);
    }

    private void ContentOnRenamed(object sender, RenamedEventArgs e)
    {
        // 如果在标签页内已经打开了, 不做处理
        var target = FindFileItemByPath(e.OldFullPath, Items);
        if (target is null)
        {
            Log.Warn("找不到改名前的项目: {FullPath}", e.OldFullPath);
            return;
        }

        var parent = target.Parent;
        if (parent is null)
        {
            Log.Warn("找不到父节点: {FullPath}", e.OldFullPath);
            return;
        }

        parent.RemoveChild(target);
        var newItem = new SystemFileItem(e.FullPath, target.IsFile, parent);
        if (newItem.IsFolder)
        {
            LoadFileSystem(e.FullPath, newItem);
        }

        var insertIndex = FindInsertIndex(newItem);
        parent.InsertChild(insertIndex, newItem);
    }

    private void ContentOnDeleted(object sender, FileSystemEventArgs e)
    {
        var target = FindFileItemByPath(e.FullPath, Items);
        if (target is null)
        {
            Log.Warn("找不到: {FullPath}", e.FullPath);
            return;
        }
        var parent = target.Parent;
        parent?.RemoveChild(target);
    }

    private void ContentOnCreated(object sender, FileSystemEventArgs e)
    {
        Log.Trace("Created: {FullPath}", e.FullPath);
        var directoryPath = Path.GetDirectoryName(e.FullPath);
        if (directoryPath is null)
        {
            return;
        }

        var isFile = !Directory.Exists(e.FullPath);
        var parent = FindFileItemByPath(directoryPath, Items);
        if (parent is null)
        {
            Log.Warn("找不到插入的位置: {FullPath}", directoryPath);
            return;
        }

        var item = new SystemFileItem(e.FullPath, isFile, parent);
        var insertIndex = FindInsertIndex(item);
        parent.InsertChild(insertIndex, item);
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

    /// <summary>
    /// 查找插入的位置
    /// </summary>
    /// <param name="newItem">新增的项目</param>
    /// <returns>新项目的插入位置</returns>
    /// <exception cref="ArgumentException">如果 <paramref name="newItem"/> 的父节点为<c>null</c></exception>
    private int FindInsertIndex(SystemFileItem newItem)
    {
        var parentChildren = newItem.Parent?.Children;
        if (parentChildren is null)
        {
            throw new ArgumentException("找不到父节点");
        }

        if (parentChildren.Count == 0)
        {
            return 0;
        }

        //  如果是文件, 添加到最后一个文件之后
        var insertIndex = parentChildren.Count;
        var maxIndex = parentChildren.Count;
        var lastFolderIndex = FindLastFolderIndex(parentChildren);
        var index = 0;

        // 当新增的是文件夹时，只与文件夹比较, 如果是文件，则只与文件比较
        if (newItem.IsFolder)
        {
            maxIndex = lastFolderIndex;
            // 未找到时，添加到最后一个文件夹之后,
            insertIndex = lastFolderIndex + 1;
        }
        else
        {
            // 跳过所有文件夹
            index = lastFolderIndex == 0 ? 0 : lastFolderIndex + 1;
        }

        while (index < maxIndex)
        {
            if (_fileSortComparer.Compare(newItem.FullPath, parentChildren[index].FullPath) == -1)
            {
                insertIndex = index;
                break;
            }

            index++;
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

    private void LoadFileSystem(string path, SystemFileItem parent)
    {
        var directories = Directory.GetDirectories(path);
        var files = Directory.GetFiles(path);

        Array.Sort(directories, _fileSortComparer);
        Array.Sort(files, _fileSortComparer);

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
