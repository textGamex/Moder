using Microsoft.UI.Xaml.Controls;
using Moder.Core.ViewsModels.Menus;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Moder.Core.Views.Menus;

public sealed partial class OpenFolderControlView : UserControl
{
	public OpenFolderControlView(OpenFolderControlViewModel model)
	{
		this.InitializeComponent();
		DataContext = model;
	}
}