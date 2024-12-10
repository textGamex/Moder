#if WINDOWS
using System.Runtime.Versioning;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

namespace Moder.Core.Services.FileNativeService;

public sealed class WindowsFileNativeService : IFileNativeService
{
    ///<inheritdoc />
    [SupportedOSPlatform("windows")]
    public bool TryMoveToRecycleBin(string fileOrDirectoryPath, out string? errorMessage, out int errorCode)
    {
        // 可以使用 dynamic
        // from https://learn.microsoft.com/en-us/windows/win32/api/shldisp/ne-shldisp-shellspecialfolderconstants

        if (!Path.Exists(fileOrDirectoryPath))
        {
            errorMessage = "文件或文件夹不存在";
            errorCode = 0;
            return false;
        }

        using var operation = new ShellFileOperations();
        operation.Options =
            ShellFileOperations.OperationFlags.RecycleOnDelete
            | ShellFileOperations.OperationFlags.NoConfirmation;
        operation.QueueDeleteOperation(new ShellItem(fileOrDirectoryPath));

        var result = default(HRESULT);
        operation.PostDeleteItem += (_, args) => result = args.Result;
        operation.PerformOperations();

        errorMessage = result.FormatMessage();
        errorCode = result.Code;

        return result.Succeeded;
    }
}
#endif
