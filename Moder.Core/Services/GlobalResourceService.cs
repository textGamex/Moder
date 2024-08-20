using System.ComponentModel.Design;
using Moder.Core.ViewsModels.Menus;

namespace Moder.Core.Services;

public sealed class GlobalResourceService
{
	private SystemFileItem? _currentSelectFileItem;

	/// <summary>
	/// 弹出 <see cref="SystemFileItem"/>, 如果未设置则抛出异常
	/// </summary>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException">未设置<c>CurrentSelectFileItem</c>时</exception>
	public SystemFileItem PopCurrentSelectFileItem()
	{
		var item =
			_currentSelectFileItem
			?? throw new InvalidOperationException("未设置CurrentSelectFileItem");
		_currentSelectFileItem = null;
		return item;
	}

	public void SetCurrentSelectFileItem(SystemFileItem fileItem)
	{
		_currentSelectFileItem = fileItem;
	}
}