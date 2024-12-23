namespace Moder.Core.Models;

public sealed class AppThemeInfo(string displayName, ThemeMode mode)
{
    public string DisplayName { get; } = displayName;
    public ThemeMode Mode { get; } = mode;
}
