using Microsoft.Extensions.Logging;
using Moder.Core.Extensions;
using Moder.Core.Parser;
using Moder.Core.Services.Config;
using ParadoxPower.CSharp;
using ParadoxPower.Process;

namespace Moder.Core.Services;

public sealed class GameResourcesService
{
    public string[] StateCategory { get; }

    private readonly GlobalSettingService _settingService;
    private readonly ILogger<GameResourcesService> _logger;
    private readonly GameModDescriptorService _descriptor;

    private static class Keywords
    {
        public const string Common = "common";
    }

    public GameResourcesService(
        GlobalSettingService settingService,
        ILogger<GameResourcesService> logger,
        GameModDescriptorService descriptor
    )
    {
        _logger = logger;
        _descriptor = descriptor;
        _settingService = settingService;

        _logger.LogInformation("开始加载游戏资源...");
        StateCategory = LoadStateCategory();
    }

    private string[] LoadStateCategory()
    {
        var filePaths = GetAllFilePriorModByRelativePathForFolder(Keywords.Common, "state_category");
        var stateCategories = new List<string>(8);

        foreach (var filePath in filePaths)
        {
            if (!TextParser.TryParse(filePath, out var node, out var error))
            {
                _logger.LogError("文件: {path} 解析失败, 错误信息: {message}", error.Filename, error.ErrorMessage);
                continue;
            }

            if (!node.TryGetChild("state_categories", out var stateCategoryNode))
            {
                _logger.LogWarning("文件: {path} 中未找到 state_categories 节点", filePath);
                continue;
            }

            stateCategories.AddRange(stateCategoryNode.Nodes.Select(childNode => childNode.Key));
        }

        return stateCategories.ToArray();
    }

    /// <summary>
    /// 获得所有应该加载的文件绝对路径, Mod优先, 遵循 replace_path 指令
    /// </summary>
    /// <param name="folderRelativePaths"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    private IEnumerable<string> GetAllFilePriorModByRelativePathForFolder(params string[] folderRelativePaths)
    {
        var relativePath = Path.Combine(folderRelativePaths);
        _logger.LogDebug("正在加载文件夹: {Path}", relativePath);
        var modFolder = Path.Combine(_settingService.ModRootFolderPath, relativePath);
        var gameFolder = Path.Combine(_settingService.GameRootFolderPath, relativePath);

        if (!Directory.Exists(gameFolder))
        {
            throw new DirectoryNotFoundException($"找不到文件夹 {gameFolder}");
        }

        if (!Directory.Exists(modFolder))
        {
            return Directory.GetFiles(gameFolder);
        }

        if (_descriptor.ReplacePaths.Contains(relativePath))
        {
            _logger.LogDebug(
                "MOD文件夹已完全替换游戏文件夹: \n\t {GamePath} => {ModPath}",
                gameFolder.ToFilePath(),
                modFolder.ToFilePath()
            );
            return Directory.GetFiles(modFolder);
        }

        var gameFilesPath = Directory.GetFiles(gameFolder);
        var modFilesPath = Directory.GetFiles(modFolder);
        return RemoveFileOfEqualName(gameFilesPath, modFilesPath);
    }

    /// <summary>
    /// 移除重名文件, 优先移除游戏本体文件
    /// </summary>
    /// <param name="gameFilePaths"></param>
    /// <param name="modFilePaths"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static IEnumerable<string> RemoveFileOfEqualName(string[] gameFilePaths, string[] modFilePaths)
    {
        var set = new HashSet<string>(gameFilePaths.Length);

        // 优先读取Mod文件
        foreach (var filePath in modFilePaths.Concat(gameFilePaths))
        {
            var fileName = Path.GetFileName(filePath) ?? throw new ArgumentException($"无法得到文件名: {filePath}");
            if (set.Add(fileName))
            {
                yield return filePath;
            }
        }
    }

    /// <summary>
    /// 根据相对路径获得游戏或者Mod文件的绝对路径, 优先Mod
    /// </summary>
    /// <remarks>
    /// 注意: 此方法会忽略mod描述文件中的 replace_path 指令
    /// </remarks>
    /// <param name="fileRelativePath">根目录下的相对路径</param>
    /// <exception cref="FileNotFoundException">游戏和mod中均不存在</exception>
    /// <returns>文件路径</returns>
    private string GetFilePathPriorModByRelativePath(string fileRelativePath)
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
