using Moder.Core.Models;
using ParadoxPower.Parser;

namespace Moder.Core.Helper;

public static class ValueConverterHelper
{
	public static Types.Value ToValueType(GameValueType type, string value)
	{
		return type switch
		{
			GameValueType.Bool => Types.Value.NewBool(bool.Parse(value)),
			GameValueType.Float => Types.Value.NewFloat(decimal.Parse(value)),
			GameValueType.Int => Types.Value.NewInt(int.Parse(value)),
			GameValueType.String => Types.Value.NewStringValue(value),
			GameValueType.StringWithQuotation => Types.Value.NewQStringValue(value),
			GameValueType.None => throw new ArgumentException(),
			_ => throw new ArgumentException()
		};
	}
}