using Avalonia;
using HarfBuzzSharp;
using SkiaSharp;

namespace Moder.Core.Graph.DrawHelper;

public sealed class GridData
{
    public static SKPoint Origin { get; set; }
    public static SKRect PanoramaRect { get; set; }
    public static SKRect DrawRect { get; set; }
    public static float GuideLineThickness { get; set; } = 2f;
    public static SKColor GuideLineColor { get; set; } = SKColors.SlateGray;
    public static SKColor FocusColor { get; set; } =
        new(SKColors.Red.Red, SKColors.Red.Green, SKColors.Red.Blue, 200);
    public static bool ShowEmptyGrid { get; set; }
#if DEBUG
        = true;
#endif
    public static PixelPoint? SelectSite { get; set; }
    public static Direction SelectCellPart { get; set; } = Direction.None;
    public static PixelPoint? FocusSite { get; set; }
    public static bool RedrawAll { get; set; }
}