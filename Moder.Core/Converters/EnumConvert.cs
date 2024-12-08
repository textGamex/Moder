namespace Moder.Core.Converters;

public static class EnumConvert
{
    public static object? ToEnum(this string str, Type enumType)
    {
        try
        {
            if (Enum.TryParse(enumType, str, true, out var result))
            {
                return result;
                
            }
        }
        catch { }
        return null;
    }
    
    public static object? ToEnumItem(this int index, Type enumType)
    {
        var names = Enum.GetNames(enumType);
        if (index >= names.Length || index < 0)
        {
            return null;
        }
        var obj = names[index].ToEnum(enumType);
        return obj;
    }
}
