using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.ViewsModel.Menus;

namespace Moder.Core.Views.Menus;

public sealed partial class MainControlView : UserControl
{
    public MainControlView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<MainControlViewModel>();

        SideBarControl.Content = App.Services.GetRequiredService<SideBarControlView>();
        WorkSpaceControl.Content = App.Services.GetRequiredService<WorkSpaceControlView>();

        StatusBarControl.Content = App.Services.GetRequiredService<StatusBarControlView>();
    }
}
