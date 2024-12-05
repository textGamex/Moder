using Microsoft.Extensions.DependencyInjection;

namespace Moder.Core.ViewsModel;

public static class DesignData
{
    public static MainWindowViewModel MainWindowViewModel { get; } =
        App.Current.Services.GetRequiredService<MainWindowViewModel>();
    public static AppInitializeControlViewModel AppInitializeControlViewModel { get; } =
        App.Current.Services.GetRequiredService<AppInitializeControlViewModel>();
}
