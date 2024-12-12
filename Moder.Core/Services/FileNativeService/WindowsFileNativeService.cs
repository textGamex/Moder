#if WINDOWS
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Moder.Language.Strings;
using NLog;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

namespace Moder.Core.Services.FileNativeService;

public sealed class WindowsFileNativeService : IFileNativeService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    ///<inheritdoc />
    [SupportedOSPlatform("windows")]
    public bool TryMoveToRecycleBin(
        string fileOrDirectoryPath,
        [NotNullWhen(false)] out string? errorMessage,
        out int errorCode
    )
    {
        // 可以使用 dynamic
        // from https://learn.microsoft.com/en-us/windows/win32/api/shldisp/ne-shldisp-shellspecialfolderconstants

        if (!Path.Exists(fileOrDirectoryPath))
        {
            errorMessage = Resource.FileManager_NotExist;
            errorCode = 0;
            return false;
        }

        using var operation = new ShellFileOperations();
        operation.Options =
            ShellFileOperations.OperationFlags.RecycleOnDelete
            | ShellFileOperations.OperationFlags.NoConfirmation;
        using var item = new ShellItem(fileOrDirectoryPath);
        operation.QueueDeleteOperation(item);

        var result = default(HRESULT);
        operation.PostDeleteItem += (_, args) => result = args.Result;
        operation.PerformOperations();

        errorMessage = result.FormatMessage();
        errorCode = result.Code;

        return result.Succeeded;
    }

    ///<inheritdoc/>
    public bool TryShowInExplorer(
        string fileOrDirectoryPath,
        bool isFile,
        [NotNullWhen(false)] out string? errorMessage
    )
    {
        try
        {
            var startInfo = new ProcessStartInfo("explorer.exe")
            {
                UseShellExecute = true,
                Arguments = $"/select, \"{fileOrDirectoryPath}\""
            };

            using var process = Process.Start(startInfo);

            errorMessage = null;
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, $"{Resource.FileManager_OpenFailed}{fileOrDirectoryPath}");
            errorMessage = e.Message;
            return false;
        }
    }
}
#endif
