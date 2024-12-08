using Avalonia.Styling;

namespace Moder.Core.Resources;

public class AppTheme
{
    public static ThemeVariant GetThemeVariant(ThemeMode type)
    {
        return type switch
        {
            ThemeMode.Light => new(nameof(ThemeMode.Light), ThemeVariant.Light),
            ThemeMode.Dark => new(nameof(ThemeMode.Dark), ThemeVariant.Dark),
            ThemeMode.DarkSlateGray => new(nameof(ThemeMode.DarkSlateGray), ThemeVariant.Dark),
            _ => ThemeVariant.Default,
        };
    }
}
