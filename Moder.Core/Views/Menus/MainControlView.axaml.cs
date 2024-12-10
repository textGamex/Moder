using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.ViewsModel;
using Moder.Core.ViewsModel.Menus;

namespace Moder.Core.Views.Menus;

public partial class MainControlView : UserControl
{
    public MainControlView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<MainControlViewModel>();
        
        SideBarControl.Content = App.Services.GetRequiredService<SideBarControlView>();
        WorkSpaceControl.Content = App.Services.GetRequiredService<WorkSpaceControlView>();
    }
}