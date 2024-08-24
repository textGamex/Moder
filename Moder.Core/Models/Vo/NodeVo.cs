namespace Moder.Core.Models.Vo;

public class NodeVo(string key) : ObservableGameValue(key)
{
	public IReadOnlyList<ObservableGameValue> Children => _children;
	private readonly List<ObservableGameValue> _children = new(8);

	public void AddChild(ObservableGameValue child)
	{
		_children.Add(child);
	}
}