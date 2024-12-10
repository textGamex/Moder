using Avalonia.Styling;

namespace Moder.Core.Resources;

public static class ThemeVariants
{
    public static ThemeVariant Light { get; } = AppTheme.GetThemeVariant(ThemeMode.Light);

    public static ThemeVariant Dark { get; } = AppTheme.GetThemeVariant(ThemeMode.Dark);

    public static ThemeVariant DarkSlateGray { get; } = AppTheme.GetThemeVariant(ThemeMode.DarkSlateGray);
}
