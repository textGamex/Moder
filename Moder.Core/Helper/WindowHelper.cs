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
        SystemBackdrop backdrop = settings.WindowBackdropType switch
        {
            //TODO: 有可能不支持
            WindowBackdropType.Default
            or WindowBackdropType.Mica
                => new MicaBackdrop(),
            WindowBackdropType.MicaAlt => new MicaBackdrop { Kind = MicaKind.BaseAlt },
            WindowBackdropType.Acrylic => new DesktopAcrylicBackdrop(),
            _ => throw new ArgumentOutOfRangeException()
        };
        window.SystemBackdrop = backdrop;
    }
}
