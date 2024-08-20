using Moder.Core.ViewsModels.Game;

namespace Moder.Core.Views.Game;

public sealed partial class StateFileControlView
{
	public StateFileControlViewModel ViewModel => (StateFileControlViewModel)DataContext;

	public StateFileControlView(StateFileControlViewModel model)
	{
		InitializeComponent();
		DataContext = model;
	}
}