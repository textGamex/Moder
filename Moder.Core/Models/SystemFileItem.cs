﻿using System.Collections.ObjectModel;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services;
using Moder.Core.Services.Config;
using Moder.Core.Services.FileNativeService;
using Moder.Core.Views.Menus;
using Moder.Language.Strings;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NLog;

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
    private static readonly IFileNativeService FileNativeService =
        App.Services.GetRequiredService<IFileNativeService>();
    private static readonly AppSettingService AppSettingService =
        App.Services.GetRequiredService<AppSettingService>();

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

    [RelayCommand]
    private void ShowInExplorer()
    {
        _ = FileNativeService.TryShowInExplorer(FullPath, IsFile, out _);
    }

    [RelayCommand]
    private async Task RenameAsync()
    {
        var dialog = new ContentDialog
        {
            Title = Resource.Common_Rename,
            PrimaryButtonText = Resource.Common_Ok,
            CloseButtonText = Resource.Common_Cancel,
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

        try
        {
            Rename(newPath);
        }
        catch (Exception e)
        {
            Log.Error(e, "重命名文件或文件夹时发生错误");
            await MessageBoxService.ErrorAsync(Resource.RenameFile_ErrorOccurs);
        }
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
    private async Task CopyPath()
    {
        await CopyToClipboard(FullPath).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task CopyAsRelativePath()
    {
        var relativePath = Path.GetRelativePath(AppSettingService.ModRootFolderPath, FullPath);
        await CopyToClipboard(relativePath).ConfigureAwait(false);
    }

    private static async Task CopyToClipboard(string path)
    {
        var app = (ClassicDesktopStyleApplicationLifetime?)App.Current.ApplicationLifetime;
        if (app?.MainWindow?.Clipboard is null)
        {
            Log.Warn("无法复制文件路径，剪切板不可用");
            return;
        }

        await app.MainWindow.Clipboard.SetTextAsync(path);
    }

    [RelayCommand]
    private async Task DeleteFile()
    {
        var text = IsFile
            ? string.Format(Resource.DeleteFile_EnsureFile, Name)
            : string.Format(Resource.DeleteFile_EnsureFolder, Name);
        text += $"\n\n{Resource.DeleteFile_CanFindBack}";
        var dialog = MessageBoxManager.GetMessageBoxStandard(Resource.Common_Delete, text, ButtonEnum.YesNo);

        var result = await dialog.ShowAsync();
        if (result == ButtonResult.Yes)
        {
            if (FileNativeService.TryMoveToRecycleBin(FullPath, out var errorMessage, out var errorCode))
            {
                Parent?._children.Remove(this);
            }
            else
            {
                await MessageBoxService.ErrorAsync($"{Resource.DeleteFile_Failed}{errorMessage}");
                Log.Warn(
                    "删除文件或文件夹失败：{FullPath}, 错误信息: {ErrorMessage} 错误代码: {Code}",
                    FullPath,
                    errorMessage,
                    errorCode
                );
            }
        }
    }
}
