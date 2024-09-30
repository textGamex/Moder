using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Moder.Core.Services.Config;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class SideWorkSpaceControlViewModel : ObservableObject
{
    [ObservableProperty]
    private IEnumerable<SystemFileItem>? _items;

    public SideWorkSpaceControlViewModel(GlobalSettingService globalSettings)
    {
        var items = new SystemFileItem(globalSettings.ModRootFolderPath, false);
        LoadFileSystem(globalSettings.ModRootFolderPath, items);
        Items = [items];
    }

    private static void LoadFileSystem(string path, SystemFileItem parent)
    {
        var directories = Directory.GetDirectories(path);
        var files = Directory.GetFiles(path);

        Array.Sort(directories, WindowsStringComparer.Instance);
        Array.Sort(files, WindowsStringComparer.Instance);

        foreach (var directoryPath in directories)
        {
            var item = new SystemFileItem(directoryPath, false);
            parent.Children.Add(item);
            LoadFileSystem(directoryPath, item);
        }

        foreach (var filePath in files)
        {
            parent.Children.Add(new SystemFileItem(filePath, true));
        }
    }

    private sealed partial class WindowsStringComparer : IComparer<string>
    {
        public static WindowsStringComparer Instance { get; } = new();

        // CSharp 实现 https://www.codeproject.com/Articles/11016/Numeric-String-Sort-in-C?msg=1183262#xx1183262xx
        [LibraryImport("shlwapi.dll", SetLastError = false, StringMarshalling = StringMarshalling.Utf16)]
        private static partial int StrCmpLogicalW(string x, string y);

        public int Compare(string? x, string? y)
        {
            if (string.IsNullOrEmpty(x))
            {
                if (string.IsNullOrEmpty(y))
                {
                    return 0;
                }

                return -1;
            }
            if (string.IsNullOrEmpty(y))
            {
                return 1;
            }

            return StrCmpLogicalW(x, y);
        }
    }
}
