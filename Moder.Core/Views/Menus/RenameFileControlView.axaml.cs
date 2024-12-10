using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Moder.Core.Models;
using Moder.Core.ViewsModel.Menus;

namespace Moder.Core.Views.Menus;

public partial class RenameFileControlView : UserControl
{
    public string NewName => ViewModel.NewName;
    public bool IsInvalid => !ViewModel.IsValid;
    private RenameFileControlViewModel ViewModel { get; }

    public RenameFileControlView(ContentDialog dialog, SystemFileItem fileItem)
    {
        InitializeComponent();
        ViewModel = new RenameFileControlViewModel(dialog, fileItem);
        DataContext = ViewModel;
    }
}
