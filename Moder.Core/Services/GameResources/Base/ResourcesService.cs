using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moder.Core.Services.Config;

namespace Moder.Core.Services.GameResources.Base;

public abstract partial class ResourcesService<TType, TContent, TParseResult> : IResourcesService
    where TType : ResourcesService<TType, TContent, TParseResult>
{
    protected readonly string FolderRelativePath;

    /// <summary>
    /// key: 文件路径, value: 文件内资源内容
    /// </summary>
    protected readonly Dictionary<string, TContent> Resources;
    protected readonly ILogger<TType> Logger;
    protected event EventHandler<ResourceChangedEventArgs>? OnResourceChanged;

    private readonly GlobalSettingService _settingService;

    protected ResourcesService(string folderRelativePath, WatcherFilter filter)
    {
        FolderRelativePath = folderRelativePath;
        Logger = App.Current.Services.GetRequiredService<ILogger<TType>>();
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
        Logger.LogInformation(
            "初始化资源成功: {FolderRelativePath}, 共 {Count} 个文件",
            FolderRelativePath,
            filePaths.Count
        );
        LogItemsSum();
    }

    [Conditional("DEBUG")]
    private void LogItemsSum()
    {
        if (typeof(IReadOnlyCollection<object>).IsAssignableFrom(typeof(TContent)))
        {
            Logger.LogDebug(
                "'{}'下资源数量: {Count}",
                FolderRelativePath,
                Resources.Values.Cast<IReadOnlyCollection<object>>().Sum(content => content.Count)
            );
        }
    }

    public void Add(string folderOrFilePath)
    {
        Logger.LogDebug("添加 Mod 资源: {FolderOrFilePath}", folderOrFilePath);
        Debug.Assert(File.Exists(folderOrFilePath), "必须为文件");

        // 如果新增加的mod资源在原版资源中存在, 移除原版资源, 添加mod资源
        var relativeFilePath = Path.GetRelativePath(_settingService.ModRootFolderPath, folderOrFilePath);
        var gameFilePath = Path.Combine(_settingService.GameRootFolderPath, relativeFilePath);
        var isRemoved = Resources.Remove(gameFilePath);
        if (isRemoved)
        {
            Logger.LogInformation("移除游戏资源成功: {GameFilePath}", gameFilePath);
        }

        ParseFileAndAddToResources(folderOrFilePath);
        OnOnResourceChanged(new ResourceChangedEventArgs(folderOrFilePath));

        Logger.LogInformation("添加 Mod 资源成功: {FolderOrFilePath}", folderOrFilePath);
    }

    public void Remove(string folderOrFilePath)
    {
        Logger.LogDebug("移除 Mod 资源: {FolderOrFilePath}", folderOrFilePath);
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
            Logger.LogInformation("移除 Mod 资源成功");
            var relativeFilePath = Path.GetRelativePath(_settingService.ModRootFolderPath, folderOrFilePath);

            // 如果删除的mod资源在原版资源中存在, 移除mod资源, 添加原版资源
            var gameFilePath = Path.Combine(_settingService.GameRootFolderPath, relativeFilePath);
            if (!File.Exists(gameFilePath))
            {
                return;
            }

            ParseFileAndAddToResources(gameFilePath);
            OnOnResourceChanged(new ResourceChangedEventArgs(folderOrFilePath));

            Logger.LogInformation("添加原版游戏资源: {GameFilePath}", gameFilePath);
        }
    }

    public void Reload(string folderOrFilePath)
    {
        Logger.LogDebug("尝试重新加载 Mod 资源: {FolderOrFilePath}", folderOrFilePath);
        if (Directory.Exists(folderOrFilePath))
        {
            Logger.LogDebug("跳过文件夹");
            return;
        }

        var isRemoved = Resources.Remove(folderOrFilePath);
        Debug.Assert(isRemoved, "Mod 资源不存在, 但尝试重新加载");
        ParseFileAndAddToResources(folderOrFilePath);
        OnOnResourceChanged(new ResourceChangedEventArgs(folderOrFilePath));

        Logger.LogInformation("重新加载 Mod 资源成功");
    }

    public void Renamed(string oldPath, string newPath)
    {
        Logger.LogDebug("Mod 资源重命名: {OldPath} -> {NewPath}", oldPath, newPath);
        if (Directory.Exists(newPath))
        {
            Logger.LogDebug("跳过文件夹");
            return;
        }

        if (Resources.TryGetValue(oldPath, out var countryTags))
        {
            Resources.Add(newPath, countryTags);
        }
        else
        {
            ParseFileAndAddToResources(newPath);
        }
        var isRemoved = Resources.Remove(oldPath);

        Debug.Assert(isRemoved, "Mod 资源不存在, 但尝试重命名");
        Logger.LogInformation("Mod 资源重命名成功");
    }

    /// <summary>
    /// 解析文件
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

    protected sealed class ResourceChangedEventArgs : EventArgs
    {
        public string FilePath { get; }

        public ResourceChangedEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}
