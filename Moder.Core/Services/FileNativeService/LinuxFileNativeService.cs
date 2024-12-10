#if LINUX
using System.Text;
using System.Web;

namespace Moder.Core.Services.FileNativeService;

public sealed class LinuxFileNativeService : IFileNativeService
{
    // XDG 规范
    // 在 Ubuntu 下测试
    // https://cgit.freedesktop.org/xdg/xdg-specs/plain/trash/trash-spec.xml
    private const string TrashDir = ".local/share/Trash";
    private const string FilesDir = "files";
    private const string InfoDir = "info";

    public bool TryMoveToRecycleBin(string fileOrDirectoryPath, out string? errorMessage, out int errorCode)
    {
        try
        {
            if (!Path.Exists(fileOrDirectoryPath))
            {
                errorMessage = "文件或文件夹不存在";
                errorCode = 1;
                return false;
            }

            return TryMoveToRecycleBinCore(fileOrDirectoryPath, out errorMessage, out errorCode);
        }
        catch (Exception e)
        {
            errorMessage = e.Message;
            errorCode = 1;
        }

        return false;
    }

    private static bool TryMoveToRecycleBinCore(
        string fileOrDirectoryPath,
        out string? errorMessage,
        out int errorCode
    )
    {
        var isFile = File.Exists(fileOrDirectoryPath);

        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var trashPath = Path.Combine(homeDir, TrashDir);
        var trashFilesPath = Path.Combine(trashPath, FilesDir);
        var trashInfoPath = Path.Combine(trashPath, InfoDir);

        // Ensure the trash directories exist
        Directory.CreateDirectory(trashFilesPath);
        Directory.CreateDirectory(trashInfoPath);

        var fileOrDirectoryName = Path.GetFileName(fileOrDirectoryPath);

        var destPath = Path.Combine(trashFilesPath, fileOrDirectoryName);
        var uniqueFileOrDirectoryName = GetUniqueName(destPath);

        // Create .trashinfo metadata file
        var infoFilePath = Path.Combine(
            trashInfoPath,
            $"{Path.GetFileName(uniqueFileOrDirectoryName)}.trashinfo"
        );
        CreateTrashInfoFile(infoFilePath, fileOrDirectoryPath);

        if (isFile)
        {
            File.Move(fileOrDirectoryPath, uniqueFileOrDirectoryName);
        }
        else
        {
            Directory.Move(fileOrDirectoryPath, uniqueFileOrDirectoryName);
        }

        errorMessage = null;
        errorCode = 0;
        return true;
    }

    private static string GetUniqueName(string basePath)
    {
        var uniquePath = basePath;
        var counter = 1;
        while (Path.Exists(uniquePath))
        {
            uniquePath = $"{basePath}.{counter}";
            counter++;
        }

        return uniquePath;
    }

    private static void CreateTrashInfoFile(string infoFilePath, string originalPath)
    {
        var path = HttpUtility.UrlEncode(originalPath, Encodings.Utf8NotBom);
        // 不转也能正常工作, Ubuntu 是转了的
        path = path.Replace("%2f", "/");

        using StreamWriter writer = new(infoFilePath, false, Encodings.Utf8NotBom);
        writer.WriteLine("[Trash Info]");
        writer.WriteLine($"Path={path}");
        writer.WriteLine($"DeletionDate={DateTime.Now:s}");
    }
}
#endif
