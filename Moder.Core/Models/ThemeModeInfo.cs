using Microsoft.UI.Xaml;

namespace Moder.Core.Models;

public sealed class ThemeModeInfo(string name, ElementTheme mode)
{
    public string Name { get; } = name;
    public ElementTheme Mode { get; } = mode;
}