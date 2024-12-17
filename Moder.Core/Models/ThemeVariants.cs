using Avalonia.Styling;
using Moder.Core.Extensions;

namespace Moder.Core.Models;

public static class ThemeVariants
{
    public static ThemeVariant Light { get; } = ThemeMode.Light.ToThemeVariant();

    public static ThemeVariant Dark { get; } = ThemeMode.Dark.ToThemeVariant();

    public static ThemeVariant DarkSlateGray { get; } = ThemeMode.DarkSlateGray.ToThemeVariant();
}
