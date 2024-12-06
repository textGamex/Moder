using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services;
using NLog;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Moder.Core.Models;

public sealed partial class SystemFileItem
{
    /// <summary>
    /// 当是文件时是文件名, 文件夹时是文件夹名
    /// </summary>
    public string Name { get; }

    public string FullPath { get; }
    public bool IsFile { get; }
    public bool IsFolder => !IsFile;
    public SystemFileItem? Parent { get; }
    public IReadOnlyList<SystemFileItem> Children => _children;
    private readonly ObservableCollection<SystemFileItem> _children = [];

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private static readonly MessageBoxService MessageBoxService =
        App.Services.GetRequiredService<MessageBoxService>();

    public SystemFileItem(string fullPath, bool isFile, SystemFileItem? parent)
    {
        Name = Path.GetFileName(fullPath);
        FullPath = fullPath;
        IsFile = isFile;
        Parent = parent;
    }

    /// <summary>
    /// 添加子节点
    /// </summary>
    /// <param name="child">添加的子节点</param>
    /// <exception cref="ArgumentException">如果子节点的父节点不是当前节点, 则抛出此异常</exception>
    public void AddChild(SystemFileItem child)
    {
        if (!ReferenceEquals(child.Parent, this))
        {
            throw new ArgumentException("Child's parent should be this");
        }

        _children.Add(child);
    }

    public void AddChild(string fullPath, bool isFile)
    {
        AddChild(new SystemFileItem(fullPath, isFile, this));
    }

    public void InsertChild(int index, SystemFileItem child)
    {
        if (!ReferenceEquals(child.Parent, this))
        {
            throw new ArgumentException("Child's parent should be this");
        }

        Dispatcher.UIThread.Post(() => _children.Insert(index, child));
    }

    public void RemoveChild(SystemFileItem child)
    {
        Dispatcher.UIThread.Post(() => _children.Remove(child));
    }

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(FullPath)}: {FullPath}, {nameof(IsFile)}: {IsFile}, {nameof(Children)}: {Children}";
    }

    // [RelayCommand]
    // private async Task ShowInExplorerAsync()
    // {
    //     string? folder;
    //     IStorageItem selectedItem;
    //     if (IsFile)
    //     {
    //         selectedItem = await StorageFile.GetFileFromPathAsync(FullPath);
    //         folder = Path.GetDirectoryName(FullPath);
    //     }
    //     else
    //     {
    //         selectedItem = await StorageFolder.GetFolderFromPathAsync(FullPath);
    //         folder = Directory.GetParent(FullPath)?.FullName;
    //     }
    //
    //     if (folder is null)
    //     {
    //         Log.Warn("在资源管理器中打开失败，无法获取路径：{FullPath}", FullPath);
    //         return;
    //     }
    //
    //     await Launcher.LaunchFolderPathAsync(
    //         folder,
    //         new FolderLauncherOptions { ItemsToSelect = { selectedItem } }
    //     );
    // }

    // [RelayCommand]
    // private async Task RenameAsync()
    // {
    //     var dialog = new ContentDialog
    //     {
    //         XamlRoot = App.Current.XamlRoot,
    //         Title = "重命名",
    //         PrimaryButtonText = "确定",
    //         CloseButtonText = "取消"
    //     };
    //     
    //     var view = new RenameFileControlView(dialog, this);
    //     dialog.Content = view;
    //
    //     var result = await dialog.ShowAsync();
    //     if (result != ContentDialogResult.Primary)
    //     {
    //         Log.Debug("取消重命名");
    //         return;
    //     }
    //
    //     if (view.IsInvalid || view.NewName == Name)
    //     {
    //         return;
    //     }
    //
    //     var parentDir = Path.GetDirectoryName(FullPath);
    //     if (parentDir is null)
    //     {
    //         Log.Warn("重命名文件失败，无法获取路径：{FullPath}", FullPath);
    //         return;
    //     }
    //
    //     var newPath = Path.Combine(parentDir, view.NewName);
    //     if (Path.Exists(newPath))
    //     {
    //         Log.Warn("重命名失败，目标文件或文件夹已存在：{FullPath}", FullPath);
    //         return;
    //     }
    //
    //     Rename(newPath);
    // }

    private void Rename(string newPath)
    {
        if (IsFile)
        {
            File.Move(FullPath, newPath);
        }
        else
        {
            Directory.Move(FullPath, newPath);
        }
    }

    [RelayCommand]
    private async Task DeleteFile()
    {
        var title = IsFile ? $"确认删除 '{Name}' 吗?" : $"确认删除 '{Name}' 及其内容吗?";
        var dialog = MessageBoxManager.GetMessageBoxStandard(title, "您可以从回收站还原此文件", ButtonEnum.YesNo);

        var result = await dialog.ShowAsync();
        if (result == ButtonResult.Yes)
        {
            // if (TryMoveToRecycleBin(FullPath, out var errorMessage, out var errorCode))
            // {
            //     Parent?._children.Remove(this);
            // }
            // else
            // {
            //     await MessageBoxService.ErrorAsync($"删除失败, 原因: {errorMessage}");
            //     Log.Warn(
            //         "删除文件或文件夹失败：{FullPath}, 错误信息: {ErrorMessage} 错误代码: {Code}",
            //         FullPath,
            //         errorMessage,
            //         errorCode
            //     );
            // }
        }
    }

    /// <summary>
    /// 尝试将文件或文件夹移动到回收站
    /// </summary>
    /// <param name="fileOrDirectoryPath">文件或文件夹路径</param>
    /// <param name="errorMessage">错误信息</param>
    /// <param name="errorCode">错误代码</param>
    /// <returns>成功返回 <c>true</c>, 失败返回 <c>false</c></returns>
    // private static bool TryMoveToRecycleBin(
    //     string fileOrDirectoryPath,
    //     out string? errorMessage,
    //     out int errorCode
    // )
    // {
    //     // 可以使用 dynamic
    //     // from https://learn.microsoft.com/en-us/windows/win32/api/shldisp/ne-shldisp-shellspecialfolderconstants
    //
    //     if (!Path.Exists(fileOrDirectoryPath))
    //     {
    //         errorMessage = "文件或文件夹不存在";
    //         errorCode = 0;
    //         return false;
    //     }
    //
    //     using var operation = new ShellFileOperations();
    //     operation.Options =
    //         ShellFileOperations.OperationFlags.RecycleOnDelete
    //         | ShellFileOperations.OperationFlags.NoConfirmation;
    //     operation.QueueDeleteOperation(new ShellItem(fileOrDirectoryPath));
    //
    //     var result = default(HRESULT);
    //     operation.PostDeleteItem += (_, args) => result = args.Result;
    //     operation.PerformOperations();
    //
    //     errorMessage = result.FormatMessage();
    //     errorCode = result.Code;
    //
    //     return result.Succeeded;
    // }
}
