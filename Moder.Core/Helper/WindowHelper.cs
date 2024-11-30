using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using Moder.Core.Models;
using Moder.Core.Services.Config;
using Moder.Core.Views;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using WinUIEx;
using SystemBackdrop = Microsoft.UI.Xaml.Media.SystemBackdrop;

namespace Moder.Core.Helper;

public static class WindowHelper
{
    public static void SetSystemBackdropTypeByConfig()
    {
        SetSystemBackdropTypeByConfig(App.Current.MainWindow);
    }

    public static void SetSystemBackdropTypeByConfig(MainWindow window)
    {
        var settings = App.Current.Services.GetRequiredService<GlobalSettingService>();
        SystemBackdrop? backdrop;

        switch (settings.WindowBackdropType)
        {
            case WindowBackdropType.Default:
                backdrop = MicaController.IsSupported() ? new MicaBackdrop { Kind = MicaKind.Base } : null;
                break;
            case WindowBackdropType.Mica:
                backdrop = new MicaBackdrop { Kind = MicaKind.Base };
                break;
            case WindowBackdropType.MicaAlt:
                backdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
                break;
            case WindowBackdropType.Acrylic:
                backdrop = new DesktopAcrylicBackdrop();
                break;
            case WindowBackdropType.None:
                backdrop = null;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        window.SystemBackdrop = backdrop;
    }

    public static FolderPicker CreateFolderPicker()
    {
        var folderPicker = new FolderPicker();
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.Current.MainWindow.GetWindowHandle());
        // 不设置 FileTypeFilter 在某些 Windows 版本上会报错
        folderPicker.FileTypeFilter.Add("*");

        return folderPicker;
    }
    
    public static void SetAppTheme(ElementTheme theme)
    {
        var window = App.Current.MainWindow;
        if (window.Content is FrameworkElement root)
        {
            root.RequestedTheme = theme;
        }
    }
}
