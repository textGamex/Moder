using System.Globalization;
using System.Resources;
using Avalonia.Data.Converters;
using EnumsNET;

namespace Moder.Core.Converters;

public sealed class EnumTypeToStringListConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Type type || !typeof(Enum).IsAssignableFrom(type))
        {
            return null;
        }
        var names = Enums.GetNames(type);
        var localiza = new List<string>(names.Count);
        var resourceManager = new ResourceManager(typeof(Language.Strings.Resource));
        foreach (var n in names)
        {
            var name = $"{type.Name}.{n}";
            localiza.Add(
                resourceManager.GetString(name, Language.Strings.Resource.Culture)
                    ?? Language.Strings.Resource.LocalizeValueNotFind
            );
        }
        return localiza;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
