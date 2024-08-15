using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Moder.Core.Config;
using Moder.Core.Messages;
using Moder.Core.Views.Game;
using Moder.Core.Views.Menus;
using Moder.Core.ViewsModels.Menus;

namespace Moder.Core;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly GlobalSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel model, GlobalSettings settings, IServiceProvider serviceProvider)
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        ViewModel = model;

        if (string.IsNullOrEmpty(settings.WorkRootFolderPath))
        {
            SideContentControl.Content = serviceProvider.GetRequiredService<OpenFolderControlView>();
        }
        WeakReferenceMessenger.Default.Register<CompleteWorkFolderSelectMessage>(
            this,
            (_, _) =>
            {
                SideContentControl.Content = serviceProvider.GetRequiredService<SideWorkSpaceControlView>();
            }
        );
        WeakReferenceMessenger.Default.Register<OpenFileMessage>(
            this,
            (_, message) =>
            {
                MainFrame.Content = GetContent(message.FileItem);
            }
        );
    }

    private object GetContent(SystemFileItem fileItem)
    {
        var relativePath = Path.GetRelativePath(_settings.WorkRootFolderPath, fileItem.FullPath);
        if (relativePath.Contains("states"))
        {
            return _serviceProvider.GetRequiredService<StateFileControlView>();
        }
        return "暂不支持此类型文件";
    }
}
