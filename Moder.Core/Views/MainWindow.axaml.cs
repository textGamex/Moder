using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services.Config;
using Moder.Core.ViewsModel;
using NLog;

namespace Moder.Core.Views;

public sealed partial class MainWindow : Window
{
    private readonly AppSettingService _settingService;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public MainWindow(AppSettingService settingService)
    {
        Log.Info("App Config path: {Path}", App.AppConfigFolder);
        _settingService = settingService;
        InitializeComponent();
        
        DataContext = App.Current.Services.GetRequiredService<MainWindowViewModel>();

        if (string.IsNullOrEmpty(settingService.GameRootFolderPath))
        {
            Log.Info("开始初始化设置");
            MainContentControl.Content = App.Current.Services.GetRequiredService<AppInitializeControlView>();
        }
        else
        {
            
        }
    }
}