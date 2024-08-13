using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Moder.Core.Config;
using Moder.Core.Messages;
using Moder.Core.Views.Menus;


namespace Moder.Core;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel model, GlobalSettings settings, IServiceProvider serviceProvider)
    {
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
    }
}
