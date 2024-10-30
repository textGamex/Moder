using Microsoft.UI.Xaml.Controls;
using Moder.Core.ViewsModels.Menus;

namespace Moder.Core.Views.Menus;

public sealed partial class RenameFileControlView : UserControl
{
    public bool IsInvalid => !ViewModel.IsValid;
    public string NewName => NewNameTextBox.Text;
    private RenameFileControlViewModel ViewModel { get; }

    public RenameFileControlView(ContentDialog dialog, SystemFileItem fileItem)
    {
        ViewModel = new RenameFileControlViewModel(dialog, fileItem);
        InitializeComponent();
        
        // 在 ViewModel 里设置 SelectionLength 不生效, 在这里设置一下
        NewNameTextBox.Text = ViewModel.NewName;
        NewNameTextBox.SelectionLength = ViewModel.SelectionLength;
    }
}
