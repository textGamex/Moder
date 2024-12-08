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

    public static object? ToEnumItem(int index, Type enumType)
    {
        var names = Enums.GetNames(enumType);
        if (index >= names.Count || index < 0)
        {
            return null;
        }
        var obj = names[index].ToEnum(enumType);
        return obj;
    }
}
