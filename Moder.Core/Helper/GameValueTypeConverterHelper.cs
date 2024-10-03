using Moder.Core.Models;

namespace Moder.Core.Helper;

public static class GameValueTypeConverterHelper
{
	public static GameValueType GetTypeForString(string value)
	{
		if (int.TryParse(value, out _))
		{
			return GameValueType.Int;
		}

		if (double.TryParse(value, out _))
		{
			return GameValueType.Float;
		}

		if (
			value.Equals("yes", StringComparison.OrdinalIgnoreCase)
			|| value.Equals("no", StringComparison.OrdinalIgnoreCase)
		)
		{
			return GameValueType.Bool;
		}

		if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
		{
			return GameValueType.StringWithQuotation;
		}

		return GameValueType.String;
	}
}