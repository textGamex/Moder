using EnumsNET;
using Moder.Core.Infrastructure;
using Moder.Core.Services.Config;
using NLog;

namespace Moder.Core.Services.GameResources.Base;

/// <summary>
/// 用来监听游戏资源的改变, 如注册的国家标签, 资源, 建筑物
/// </summary>
public sealed class GameResourcesWatcherService : IDisposable
{
    /// <summary>
    /// key: 资源文件夹路径, value: 监听器列表
    /// </summary>
    private readonly Dictionary<string, List<FileSystemSafeWatcher>> _watchedPaths = new(8);
    private readonly FileSystemSafeWatcher _watcher;

    //TODO: 重构, 可以用这个类监测资源服务的变化并发出通知
    // 监听者???, 消息总线?

    /// <summary>
    /// 待监听文件夹列表, 其中的文件夹被创建或从其他名称重命名后, 会被自动监听, 然后被移除
    /// </summary>
    private readonly List<(
        string folderRelativePath,
        IResourcesService resourcesService,
        string filter,
        bool includeSubFolders
    )> _waitingWatchFolders = new(8);
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private readonly AppSettingService _settingService;

    public GameResourcesWatcherService(AppSettingService settingService)
    {
        _settingService = settingService;
        _watcher = new FileSystemSafeWatcher(_settingService.ModRootFolderPath, "*.*");
        _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName;
        _watcher.Created += OnModResourceFolderCreatedOrRenamed;
        _watcher.Renamed += OnModResourceFolderCreatedOrRenamed;
        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnModResourceFolderCreatedOrRenamed(object sender, FileSystemEventArgs args)
    {
        var relativePath = Path.GetRelativePath(_settingService.ModRootFolderPath, args.FullPath);
        var index = _waitingWatchFolders.FindIndex(tuple => tuple.folderRelativePath == relativePath);
        if (index != -1)
        {
            var (_, resourcesService, filter, includeSubFolders) = _waitingWatchFolders[index];
            Watch(relativePath, resourcesService, filter, includeSubFolders);
            _waitingWatchFolders.RemoveAt(index);

            Log.Info("等待监听的文件夹 '{FolderName}' 被创建或重命名, 从待监听列表中移除并开始监听", Path.GetFileName(args.FullPath));
        }
    }

    public void Watch(
        string folderRelativePath,
        IResourcesService resourcesService,
        string filter = "*.*",
        bool includeSubFolders = false
    )
    {
        var modFolderPath = Path.Combine(_settingService.ModRootFolderPath, folderRelativePath);
        // 如果 Mod文件夹 不存在, 监听上一级文件夹, 当 Mod文件夹 创建后, 自动监听
        if (!Directory.Exists(modFolderPath))
        {
            _waitingWatchFolders.Add((folderRelativePath, resourcesService, filter, includeSubFolders));

            Log.Info("Mod 目录中 '{FolderPath}' 文件夹不存在, 无法监听, 已添加到等待监听列表", folderRelativePath);
            Log.Debug("Path: {FolderPath}", modFolderPath);
            return;
        }

        var watcher = new FileSystemSafeWatcher(modFolderPath, filter);
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        watcher.Changed += (_, args) =>
        {
            if (args.ChangeType.HasAnyFlags(WatcherChangeTypes.Changed))
            {
                resourcesService.Reload(args.FullPath);
            }

            Log.Debug("资源文件: {Path} 发生变化, 类型: {ChangeType}", args.FullPath, args.ChangeType);
#if DEBUG
            if (
                args.ChangeType.HasAnyFlags(WatcherChangeTypes.Changed)
                && args.ChangeType.HasAnyFlags(
                    WatcherChangeTypes.Renamed | WatcherChangeTypes.Deleted | WatcherChangeTypes.Created
                )
            )
            {
                Log.Error("在单个事件中同时进行两项更改, Path: {Path}", args.FullPath);
            }
#endif
        };
        watcher.Renamed += (_, args) => resourcesService.Renamed(args.OldFullPath, args.FullPath);
        watcher.Created += (_, args) => resourcesService.Add(args.FullPath);
        watcher.Deleted += (_, args) => resourcesService.Remove(args.FullPath);
        watcher.IncludeSubdirectories = includeSubFolders;
        watcher.EnableRaisingEvents = true;

        if (_watchedPaths.TryGetValue(folderRelativePath, out var watcherList))
        {
            Log.Debug("多个服务监听同一文件夹, Path: {FolderPath}", modFolderPath);
            watcherList.Add(watcher);
        }
        else
        {
            _watchedPaths.Add(folderRelativePath, [watcher]);
        }

        Log.Info("开始监听资源文件夹: {FolderPath}", modFolderPath);
    }

    public void Unwatch(string folderRelativePath)
    {
        Log.Debug("尝试停止监听资源文件夹: {FolderPath}", folderRelativePath);
        if (_watchedPaths.TryGetValue(folderRelativePath, out var watcherList))
        {
            watcherList.ForEach(watcher => watcher.Dispose());
            _watchedPaths.Remove(folderRelativePath);
            var isRemoved =
                _waitingWatchFolders.RemoveAll(tuple => tuple.folderRelativePath == folderRelativePath) != 0;
            if (isRemoved)
            {
                Log.Info("从待监听文件夹列表中移除: {FolderPath}", folderRelativePath);
            }
            Log.Info("成功停止监听资源文件夹: {FolderPath}", folderRelativePath);
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
        foreach (var watcher in _watchedPaths.Values.SelectMany(watcherList => watcherList))
        {
            watcher.Dispose();
        }
    }
}
