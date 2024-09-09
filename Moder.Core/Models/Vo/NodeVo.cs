using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Moder.Core.Models.Vo;

public class NodeVo(string key) : ObservableGameValue(key)
{
	public ObservableCollection<ObservableGameValue> Children { get; } = [];

	public void Add(ObservableGameValue child)
	{
		Children.Add(child);
	}

	public void Remove(ObservableGameValue child)
	{
		var isRemoved = Children.Remove(child);
		Debug.Assert(isRemoved, "Failed to remove child from NodeVo.");
	}
}