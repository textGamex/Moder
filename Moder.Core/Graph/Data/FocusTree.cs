using Avalonia;
using Moder.Core.Graph.Tools;

namespace Moder.Core.Graph.Data;

public sealed class FocusTree
{
    public Roster<PixelPoint, FocusNode> FocusNodes { get; set; } = [];

    public PixelSize Size { get; set; }

    public int Width => Size.Width;

    public int Height => Size.Height;

    public FocusNode this[PixelPoint site]
    {
        get
        {
            if (FocusNodes.TryGetValue(site, out var sourceLand))
            {
                return sourceLand;
            }
            return new FocusNode(site, NodeType.None);
        }
    }

    public PixelPoint SetPointWithin(PixelPoint point)
    {
        return SetPointWithin(point, Size);
    }

    private static PixelPoint SetPointWithin(PixelPoint point, PixelSize range)
    {
        var x = point.X % range.Width;
        if (x < 0)
        {
            x += range.Width;
        }
        var y = point.Y % range.Height;
        if (y < 0)
        {
            y += range.Height;
        }
        return new(x, y);
    }
}
