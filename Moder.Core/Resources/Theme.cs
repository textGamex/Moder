using Avalonia.Styling;

namespace Moder.Core.Resources;

public static class Theme
{
    public static ThemeVariant GetThemeVariant(this ThemeVariantTypes type)
    {
        return type switch
        {
            ThemeVariantTypes.Light => new(nameof(ThemeVariantTypes.Light), ThemeVariant.Light),
            ThemeVariantTypes.Dark => new(nameof(ThemeVariantTypes.Dark), ThemeVariant.Dark),
            ThemeVariantTypes.DarkSlateGray => new(nameof(ThemeVariantTypes.DarkSlateGray), ThemeVariant.Dark),
            _ => ThemeVariant.Default,
        };
    }
}