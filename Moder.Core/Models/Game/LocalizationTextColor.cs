using Avalonia.Media;

namespace Moder.Core.Models.Game;

public sealed class LocalizationTextColor(char key, Color color)
{
    public char Key { get; } = key;
    public Color Color { get; } = color;
}
