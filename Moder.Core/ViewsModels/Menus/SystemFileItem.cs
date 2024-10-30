using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Views.Menus;
using NLog;
using Windows.Storage;
using Windows.System;

namespace Moder.Core.ViewsModels.Menus;

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

        App.Current.DispatcherQueue.TryEnqueue(() => _children.Insert(index, child));
    }

    public void RemoveChild(SystemFileItem child)
    {
        App.Current.DispatcherQueue.TryEnqueue(() => _children.Remove(child));
    }

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(FullPath)}: {FullPath}, {nameof(IsFile)}: {IsFile}, {nameof(Children)}: {Children}";
    }

    [RelayCommand]
    private async Task ShowInExplorerAsync()
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
            Log.Warn("在资源管理器中打开失败，无法获取路径：{FullPath}", FullPath);
            return;
        }

        await Launcher.LaunchFolderPathAsync(
            folder,
            new FolderLauncherOptions { ItemsToSelect = { selectedItem } }
        );
    }

    [RelayCommand]
    private async Task RenameAsync()
    {
        var dialog = new ContentDialog
        {
            XamlRoot = App.Current.XamlRoot,
            Title = "重命名",
            PrimaryButtonText = "确定",
            CloseButtonText = "取消"
        };
        var view = new RenameFileControlView(dialog, this);
        dialog.Content = view;

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            Log.Debug("取消重命名");
            return;
        }

        if (view.IsInvalid || view.NewName == Name)
        {
            return;
        }

        var parentDir = Path.GetDirectoryName(FullPath);
        if (parentDir is null)
        {
            Log.Warn("重命名文件失败，无法获取路径：{FullPath}", FullPath);
            return;
        }

        var newPath = Path.Combine(parentDir, view.NewName);
        if (Path.Exists(newPath))
        {
            Log.Warn("重命名失败，目标文件或文件夹已存在：{FullPath}", FullPath);
            return;
        }

        Rename(newPath);
    }

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
        var dialog = new ContentDialog
        {
            XamlRoot = App.Current.MainWindow.Content.XamlRoot,
            Title = IsFile ? $"确认删除 '{Name}' 吗?" : $"确认删除 '{Name}' 及其内容吗?",
            Content = "您可以从回收站还原此文件",
            PrimaryButtonText = "移动到回收站",
            CloseButtonText = "取消"
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            if (TryMoveToRecycleBin(FullPath))
            {
                Parent?._children.Remove(this);
            }
        }
    }

    /// <summary>
    /// 尝试删除文件或文件夹
    /// </summary>
    /// <param name="fileOrDirectoryPath"></param>
    /// <returns>成功返回 <c>true</c>, 失败返回 <c>false</c></returns>
    private static bool TryMoveToRecycleBin(string fileOrDirectoryPath)
    {
        // 可以使用 dynamic
        // // from https://learn.microsoft.com/en-us/windows/win32/api/shldisp/ne-shldisp-shellspecialfolderconstants
        // const int ssfBITBUCKET = 0xa;
        // dynamic? shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application")!);
        // var recycleBin = shell?.Namespace(ssfBITBUCKET);
        // recycleBin?.MoveHere(fileOrDirectoryPath);

        if (!Path.Exists(fileOrDirectoryPath))
        {
            return false;
        }

        var shf = new SHFILEOPSTRUCT
        {
            wFunc = FO_DELETE,
            fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION,
            // 需要双null结尾
            pFrom = fileOrDirectoryPath + '\0'
        };
        var errorCode = SHFileOperation(ref shf);
        if (errorCode != Ok)
        {
            Log.Warn("删除文件失败, Error Code: {Code}", errorCode);
            return false;
        }

        return true;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;

        [MarshalAs(UnmanagedType.U4)]
        public int wFunc;
        public string pFrom;
        public string pTo;
        public short fFlags;

        [MarshalAs(UnmanagedType.Bool)]
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        public string lpszProgressTitle;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHFileOperation(ref SHFILEOPSTRUCT fileOp);

    private const int FO_DELETE = 3;
    private const int FOF_ALLOWUNDO = 0x40;
    private const int FOF_NOCONFIRMATION = 0x10;
    private const int Ok = 0;
}
