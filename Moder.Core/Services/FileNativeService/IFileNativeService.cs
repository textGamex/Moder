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
}
