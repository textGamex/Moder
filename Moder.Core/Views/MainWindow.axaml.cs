using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Messages;
using Moder.Core.Services.Config;
using Moder.Core.ViewsModel;
using NLog;

namespace Moder.Core.Views;

public sealed partial class MainWindow : Window
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public MainWindow()
    {
        var settingService = App.Services.GetRequiredService<AppSettingService>();
        Log.Info("App Config path: {Path}", App.AppConfigFolder);
        InitializeComponent();

        DataContext = App.Services.GetRequiredService<MainWindowViewModel>();

        if (string.IsNullOrEmpty(settingService.GameRootFolderPath))
        {
            Log.Info("开始初始化设置");
            NavigateTo(typeof(Menus.AppInitializeControlView));
        }
        else
        {
            NavigateTo(typeof(Menus.MainControlView));
        }

        WeakReferenceMessenger.Default.Register<CompleteAppInitializeMessage>(
            this,
            (_, _) =>
            {
                NavigateTo(typeof(Menus.MainControlView));
            }
        );
    }

    private void NavigateTo(Type view)
    {
        if (MainContentControl.Content is IDisposable disposable)
        {
            disposable.Dispose();
        }

        MainContentControl.Content = App.Services.GetRequiredService(view);
        Log.Info("导航到 {View}", view.Name);
    }
}
