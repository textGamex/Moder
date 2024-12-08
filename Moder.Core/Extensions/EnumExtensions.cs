using EnumsNET;

namespace Moder.Core.Extensions;

public static class EnumExtensions
{
    public static object? ToEnum(this string str, Type enumType)
    {
        try
        {
            if (Enums.TryParse(enumType, str, true, out var result))
            {
                return result;
            }
        }
        catch { }
        return null;
    }
}
