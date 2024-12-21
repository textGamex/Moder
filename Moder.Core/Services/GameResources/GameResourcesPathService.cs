﻿using Moder.Core.Extensions;
using Moder.Core.Services.Config;
using NLog;

namespace Moder.Core.Services.GameResources;

public sealed class GameResourcesPathService
{
    private readonly AppSettingService _settingService;
    private readonly GameModDescriptorService _descriptor;

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public GameResourcesPathService(AppSettingService settingService, GameModDescriptorService descriptor)
    {
        _settingService = settingService;
        _descriptor = descriptor;
    }

    public IReadOnlyCollection<string> GetAllFilePriorModByRelativePathForFolder(
        params string[] folderRelativePaths
    )
    {
        var relativePath = Path.Combine(folderRelativePaths);
        return GetAllFilePriorModByRelativePathForFolder(relativePath);
    }

    /// <summary>
    /// 获得所有应该加载的文件绝对路径, Mod优先, 遵循 replace_path 指令
    /// </summary>
    /// <param name="folderRelativePath"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public IReadOnlyCollection<string> GetAllFilePriorModByRelativePathForFolder(
        string folderRelativePath,
        string filter = "*.*"
    )
    {
        Log.Info("正在获取文件夹 {Path} 下的文件", folderRelativePath);
        var modFolder = Path.Combine(_settingService.ModRootFolderPath, folderRelativePath);
        var gameFolder = Path.Combine(_settingService.GameRootFolderPath, folderRelativePath);

        if (!Directory.Exists(gameFolder))
        {
            throw new DirectoryNotFoundException($"找不到文件夹 {gameFolder}");
        }

        if (!Directory.Exists(modFolder))
        {
            return Directory.GetFiles(gameFolder, filter);
        }

        if (_descriptor.ReplacePaths.Contains(folderRelativePath))
        {
            Log.Debug(
                "MOD文件夹已完全替换游戏文件夹: \n\t {GamePath} => {ModPath}",
                gameFolder.ToFilePath(),
                modFolder.ToFilePath()
            );
            return Directory.GetFiles(modFolder, filter);
        }

        var gameFilesPath = Directory.GetFiles(gameFolder, filter);
        var modFilesPath = Directory.GetFiles(modFolder, filter);
        return RemoveFileOfEqualName(gameFilesPath, modFilesPath);
    }

    /// <summary>
    /// 移除重名文件, 优先移除游戏本体文件
    /// </summary>
    /// <param name="gameFilePaths"></param>
    /// <param name="modFilePaths"></param>
    /// <returns>不重名的文件路径</returns>
    /// <exception cref="ArgumentException"></exception>
    private static IReadOnlyCollection<string> RemoveFileOfEqualName(
        string[] gameFilePaths,
        string[] modFilePaths
    )
    {
        var set = new Dictionary<string, string>(Math.Max(gameFilePaths.Length, modFilePaths.Length));

        // 优先读取Mod文件
        // TODO: 做一下性能测试, 看和原来的算法有什么区别
        foreach (var filePath in modFilePaths.Concat(gameFilePaths))
        {
            var fileName = Path.GetFileName(filePath) ?? throw new ArgumentException($"无法得到文件名: {filePath}");
            set.TryAdd(fileName, filePath);
        }

        return set.Values;
    }

    /// <summary>
    /// 根据相对路径获得游戏或者Mod文件的绝对路径, 优先Mod
    /// </summary>
    /// <remarks>
    /// 注意: 此方法会忽略mod描述文件中的 replace_path 指令
    /// </remarks>
    /// <param name="fileRelativePath">根目录下的相对路径</param>
    /// <exception cref="FileNotFoundException">游戏和mod中均不存在</exception>
    /// <returns>文件绝对路径</returns>
    public string GetFilePathPriorModByRelativePath(string fileRelativePath)
    {
        var modFilePath = Path.Combine(_settingService.ModRootFolderPath, fileRelativePath);
        if (File.Exists(modFilePath))
        {
            return modFilePath;
        }

        var gameFilePath = Path.Combine(_settingService.GameRootFolderPath, fileRelativePath);
        if (File.Exists(gameFilePath))
        {
            return gameFilePath;
        }

        throw new FileNotFoundException($"在Mod和游戏中均找不到目标文件 '{fileRelativePath}'");
    }
}
