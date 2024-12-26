using Microsoft.Extensions.DependencyInjection;

namespace Moder.Core.ViewsModel;

public static class DesignData
{
    public static MainWindowViewModel MainWindowViewModel { get; } =
        App.Services.GetRequiredService<MainWindowViewModel>();
    public static Menus.AppInitializeControlViewModel AppInitializeControlViewModel { get; } =
        App.Services.GetRequiredService<Menus.AppInitializeControlViewModel>();
}
