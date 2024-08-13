using Microsoft.UI.Xaml.Controls;
using Moder.Core.ViewsModels.Menus;

namespace Moder.Core.Views.Menus;

public sealed partial class SideWorkSpaceControlView : UserControl
{
	public SideWorkSpaceControlView(SideWorkSpaceControlViewModel model)
	{
		InitializeComponent();

		DataContext = model;
	}
}