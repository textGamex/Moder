using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using Moder.Core.Models;
using Moder.Core.Services.Config;
using Moder.Core.Views;

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
}
