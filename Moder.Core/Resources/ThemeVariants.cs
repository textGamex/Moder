using Avalonia.Styling;

namespace Moder.Core.Resources;

public static class ThemeVariants
{
    public static ThemeVariant DarkSlateGray { get; } = new(nameof(DarkSlateGray), ThemeVariant.Dark);

    public static ThemeVariant Dark { get; } = new(nameof(Dark), ThemeVariant.Dark);

    public static ThemeVariant Light { get; } = new(nameof(Light), ThemeVariant.Light);
}