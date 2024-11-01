using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services.Config;
using NLog;

namespace Moder.Core.Services.GameResources.Base;

public abstract partial class ResourcesService<TType, TContent, TParseResult> : IResourcesService
    where TType : ResourcesService<TType, TContent, TParseResult>
{
    public readonly string FolderRelativePath;
    public event EventHandler<ResourceChangedEventArgs>? OnResourceChanged;

    /// <summary>
    /// key: 文件路径, value: 文件内资源内容
    /// </summary>
    protected readonly Dictionary<string, TContent> Resources;
    protected readonly Logger Logger;

    private readonly GlobalSettingService _settingService;

    protected ResourcesService(string folderRelativePath, WatcherFilter filter)
    {
        FolderRelativePath = folderRelativePath;
        Logger = LogManager.GetLogger(typeof(TType).FullName);
        _settingService = App.Current.Services.GetRequiredService<GlobalSettingService>();
        var gameResourcesPathService = App.Current.Services.GetRequiredService<GameResourcesPathService>();
        var watcherService = App.Current.Services.GetRequiredService<GameResourcesWatcherService>();

        var filePaths = gameResourcesPathService.GetAllFilePriorModByRelativePathForFolder(
            FolderRelativePath
        );

        // Resources 必须在使用 ParseFileAndAddToResources 之前初始化
        Resources = new Dictionary<string, TContent>(filePaths.Count);

        foreach (var filePath in filePaths)
        {
            ParseFileAndAddToResources(filePath);
        }

        watcherService.Watch(FolderRelativePath, this, filter.Name);
        Logger.Info("初始化资源成功: {FolderRelativePath}, 共 {Count} 个文件", FolderRelativePath, filePaths.Count);
        LogItemsSum();
    }

    [Conditional("DEBUG")]
    private void LogItemsSum()
    {
        if (typeof(IReadOnlyCollection<object>).IsAssignableFrom(typeof(TContent)))
        {
            Logger.Debug(
                "'{}'下资源数量: {Count}",
                FolderRelativePath,
                Resources.Values.Cast<IReadOnlyCollection<object>>().Sum(content => content.Count)
            );
        }
    }

    public void Add(string folderOrFilePath)
    {
        Logger.Debug("添加 Mod 资源: {FolderOrFilePath}", folderOrFilePath);
        Debug.Assert(File.Exists(folderOrFilePath), "必须为文件");

        // 如果新增加的mod资源在原版资源中存在, 移除原版资源, 添加mod资源
        var relativeFilePath = Path.GetRelativePath(_settingService.ModRootFolderPath, folderOrFilePath);
        var gameFilePath = Path.Combine(_settingService.GameRootFolderPath, relativeFilePath);
        var isRemoved = Resources.Remove(gameFilePath);
        if (isRemoved)
        {
            Logger.Info("移除游戏资源成功: {GameFilePath}", gameFilePath);
        }

        ParseFileAndAddToResources(folderOrFilePath);
        OnOnResourceChanged(new ResourceChangedEventArgs(folderOrFilePath));

        Logger.Info("添加 Mod 资源成功: {FolderOrFilePath}", folderOrFilePath);
    }

    public void Remove(string folderOrFilePath)
    {
        Logger.Debug("移除 Mod 资源: {FolderOrFilePath}", folderOrFilePath);
        if (Directory.Exists(folderOrFilePath))
        {
            foreach (
                var filePath in Directory.GetFileSystemEntries(
                    folderOrFilePath,
                    "*",
                    SearchOption.AllDirectories
                )
            )
            {
                Remove(filePath);
            }
        }

        if (Resources.Remove(folderOrFilePath))
        {
            Logger.Info("移除 Mod 资源成功");
            var relativeFilePath = Path.GetRelativePath(_settingService.ModRootFolderPath, folderOrFilePath);

            // 如果删除的mod资源在原版资源中存在, 移除mod资源, 添加原版资源
            var gameFilePath = Path.Combine(_settingService.GameRootFolderPath, relativeFilePath);
            if (!File.Exists(gameFilePath))
            {
                return;
            }

            ParseFileAndAddToResources(gameFilePath);
            OnOnResourceChanged(new ResourceChangedEventArgs(folderOrFilePath));

            Logger.Info("添加原版游戏资源: {GameFilePath}", gameFilePath);
        }
    }

    public void Reload(string folderOrFilePath)
    {
        Logger.Debug("尝试重新加载 Mod 资源: {FolderOrFilePath}", folderOrFilePath);
        if (Directory.Exists(folderOrFilePath))
        {
            Logger.Debug("跳过文件夹");
            return;
        }

        // 如果不存在, 则说明不在管理范围内, 不需要处理
        if (!Resources.ContainsKey(folderOrFilePath))
        {
            return;
        }
        
        Resources.Remove(folderOrFilePath);
        ParseFileAndAddToResources(folderOrFilePath);
        OnOnResourceChanged(new ResourceChangedEventArgs(folderOrFilePath));

        Logger.Info("重新加载 Mod 资源成功");
    }

    public void Renamed(string oldPath, string newPath)
    {
        Logger.Debug("Mod 资源重命名: {OldPath} -> {NewPath}", oldPath, newPath);
        if (Directory.Exists(newPath))
        {
            Logger.Debug("跳过文件夹");
            return;
        }
        
        if (Resources.TryGetValue(oldPath, out var countryTags))
        {
            Resources.Add(newPath, countryTags);
        }
        else
        {
            Logger.Debug("{ServiceName} 跳过处理 {NewPath} 重命名", GetType().Name, newPath);
            return;
        }
        Resources.Remove(oldPath);

        Logger.Info("Mod 资源重命名成功");
    }

    /// <summary>
    /// 解析 folderRelativePath 目录下的所有文件, 并将解析结果添加到 Resources 中
    /// </summary>
    /// <param name="result">文件解析结果</param>
    /// <returns>文件内资源内容</returns>
    protected abstract TContent? ParseFileToContent(TParseResult result);

    protected abstract TParseResult? GetParseResult(string filePath);

    private void ParseFileAndAddToResources(string filePath)
    {
        var result = GetParseResult(filePath);
        AddToResources(filePath, result);
    }

    private void AddToResources(string filePath, TParseResult? result)
    {
        if (result is null)
        {
            Logger.Warn("文件 {FilePath} 解析失败", filePath);
            return;
        }

        var content = ParseFileToContent(result);
        if (content is not null)
        {
            Resources.Add(filePath, content);
        }
    }

    private void OnOnResourceChanged(ResourceChangedEventArgs e)
    {
        OnResourceChanged?.Invoke(this, e);
    }
}
