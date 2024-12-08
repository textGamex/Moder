using Avalonia.Styling;

namespace Moder.Core.Resources;

public static class ThemeVariants
{

    public static ThemeVariant Light { get; } = ThemeVariantTypes.Light.GetThemeVariant();

    public static ThemeVariant Dark { get; } = ThemeVariantTypes.Dark.GetThemeVariant();
    public static ThemeVariant DarkSlateGray { get; } = ThemeVariantTypes.DarkSlateGray.GetThemeVariant();
}