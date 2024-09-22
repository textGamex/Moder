using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Moder.Core.Messages;
using Moder.Core.Services.Config;
using Moder.Core.Views.Game;
using Moder.Core.Views.Menus;
using Moder.Core.ViewsModels.Menus;

namespace Moder.Core;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly GlobalSettingService _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainWindow> _logger;
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(
        MainWindowViewModel model,
        GlobalSettingService settings,
        IServiceProvider serviceProvider,
        ILogger<MainWindow> logger
    )
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
        _logger = logger;
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        ViewModel = model;

        if (string.IsNullOrEmpty(settings.ModRootFolderPath))
        {
            SideContentControl.Content = serviceProvider.GetRequiredService<OpenFolderControlView>();
        }
        else
        {
            SideContentControl.Content = serviceProvider.GetRequiredService<SideWorkSpaceControlView>();
        }

        WeakReferenceMessenger.Default.Register<CompleteWorkFolderSelectMessage>(
            this,
            (_, _) => SideContentControl.Content = serviceProvider.GetRequiredService<SideWorkSpaceControlView>()
        );
        WeakReferenceMessenger.Default.Register<OpenFileMessage>(
            this,
            (_, message) => MainFrame.Content = GetContent(message.FileItem)
        );
    }

    private object GetContent(SystemFileItem fileItem)
    {
        var relativePath = Path.GetRelativePath(_settings.ModRootFolderPath, fileItem.FullPath);
        if (relativePath.Contains("states"))
        {
            return _serviceProvider.GetRequiredService<StateFileControlView>();
        }

        return "暂不支持此类型文件";
    }

    private void MainWindow_OnClosed(object sender, WindowEventArgs args)
    {
        _logger.LogInformation("配置文件保存中...");
        _settings.Save();
        _logger.LogInformation("配置文件保存完成");
    }
}
