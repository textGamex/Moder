using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.ViewsModel.Menus;

namespace Moder.Core.Views.Menus;

public sealed partial class StatusBarControlView : UserControl
{
    public StatusBarControlView()
    {
        InitializeComponent();
        
        DataContext = App.Services.GetRequiredService<StatusBarControlViewModel>();
    }
}