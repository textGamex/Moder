namespace Moder.Core.Services.FileNativeService;

public interface IFileNativeService
{
    /// <summary>
    /// 尝试将文件或文件夹移动到回收站
    /// </summary>
    /// <param name="fileOrDirectoryPath">文件或文件夹路径</param>
    /// <param name="errorMessage">错误信息</param>
    /// <param name="errorCode">错误代码</param>
    /// <returns>成功返回 <c>true</c>, 失败返回 <c>false</c></returns>
    public bool TryMoveToRecycleBin(string fileOrDirectoryPath, out string? errorMessage, out int errorCode);

    /// <summary>
    /// 尝试在资源管理器中显示文件或文件夹
    /// </summary>
    /// <param name="fileOrDirectoryPath">文件或文件夹路径</param>
    /// <param name="isFile">当<c>fileOrDirectoryPath</c>是文件路径时, 为<c>true</c>, 否则为<c>false</c></param>
    /// <param name="errorMessage">错误信息</param>
    /// <returns>成功返回 <c>true</c>, 失败返回 <c>false</c></returns>
    public bool TryShowInExplorer(string fileOrDirectoryPath, bool isFile, out string? errorMessage);
}
