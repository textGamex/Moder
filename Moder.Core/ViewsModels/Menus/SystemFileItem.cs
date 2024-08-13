using System.Collections.ObjectModel;

namespace Moder.Core.ViewsModels.Menus;

public class SystemFileItem
{
	public required string Name { get; set; }
	public required bool IsFile { get; set; }
	public ObservableCollection<SystemFileItem> Children { get; set; } = new();
}