namespace Moder.Core.Services.GameResources.Base;

/// <summary>
/// 游戏资源服务接口, 例如: 国家标签, 建筑物
/// </summary>
public interface IResourcesService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="folderOrFilePath">改变的 Mod 文件夹或文件路径</param>
    void Add(string folderOrFilePath);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="folderOrFilePath">改变的 Mod 文件夹或文件路径</param>
    void Remove(string folderOrFilePath);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="folderOrFilePath">改变的 Mod 文件夹或文件路径</param>
    void Reload(string folderOrFilePath);
    void Renamed(string oldPath, string newPath);
}