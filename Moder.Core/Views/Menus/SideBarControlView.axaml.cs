using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Messages;
using Moder.Core.Models;
using Moder.Core.ViewsModel.Menus;

namespace Moder.Core.Views.Menus;

public partial class SideBarControlView : UserControl
{
    public SideBarControlView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<SideBarControlViewModel>();
    }

    private void TreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
        {
            return;
        }

        if (e.AddedItems[0] is not SystemFileItem item)
        {
            return;
        }

        if (item.IsFolder)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new OpenFileMessage(item));
    }
}
