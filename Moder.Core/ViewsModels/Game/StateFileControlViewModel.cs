using System.Diagnostics;
using Moder.Core.Services;
using Moder.Core.ViewsModels.Menus;

namespace Moder.Core.ViewsModels.Game;

public sealed class StateFileControlViewModel
{
	public string Title => _fileItem.Name;

	private readonly SystemFileItem _fileItem;

	public StateFileControlViewModel(GlobalResourceService resourceService)
	{
		_fileItem = resourceService.PopCurrentSelectFileItem();
		Debug.Assert(_fileItem.IsFile);
	}
}