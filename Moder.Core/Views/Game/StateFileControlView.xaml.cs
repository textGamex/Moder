using Moder.Core.ViewsModels.Game;

namespace Moder.Core.Views.Game;

public sealed partial class StateFileControlView
{
	public StateFileControlView(StateFileControlViewModel model)
	{
		InitializeComponent();
		DataContext = model;
	}
}