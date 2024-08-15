using System.Collections.ObjectModel;

namespace Moder.Core.ViewsModels.Menus;

public sealed class SystemFileItem
{
	public string Name { get; }
	public string FullPath { get; }
	public bool IsFile { get; }
	public ObservableCollection<SystemFileItem> Children { get; } = [];

	public SystemFileItem(string fullPath, bool isFile)
	{
		Name = Path.GetFileName(fullPath);
		FullPath = fullPath;
		IsFile = isFile;
	}

	public override string ToString()
	{
		return
			$"{nameof(Name)}: {Name}, {nameof(FullPath)}: {FullPath}, {nameof(IsFile)}: {IsFile}, {nameof(Children)}: {Children}";
	}
}