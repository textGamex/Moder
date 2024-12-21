using Avalonia.Media;

namespace Moder.Core.Models;

public sealed class ColorTextInfo(string text, IBrush brush)
{
    public string DisplayText { get; } = text;
    public IBrush Brush { get; } = brush;
}
