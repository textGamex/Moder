using ParadoxPower.Process;

namespace Moder.Core.Extensions;

public static class ParserExtend
{
	public static bool HasNot(this Node node, string key)
	{
		return !node.Has(key);
	}

	public static Node GetChild(this Node node, string key)
	{
		return node.Child(key).Value;
	}

	public static bool TryGetChild(this Node node, string key, out Node child)
	{
		var result = node.Child(key);
		if (result is null)
		{
			child = null!;
			return false;
		}
		else
		{
			child = result.Value;
			return true;
		}
	}
}