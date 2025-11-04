using SkiaSharp;

namespace Moder.Core.Graph.DrawHelper;

public sealed class GridDrawData
{
    public SKColor ForeColor { get; }
    public SKColor BackColor { get; }
    public SKPoint Origin { get; }
    public SKRect DrawRect { get; }
    public SKRect PanoramaRect { get; }
    public int CellEdgeLength { get; }
    public float CellPaddingFactor { get; }

    public GridDrawData(SKColor foreColor, SKColor backColor)
    {
        ForeColor = foreColor;
        BackColor = backColor;
        Origin = GridData.Origin;
        DrawRect = GridData.DrawRect;
        PanoramaRect = GridData.PanoramaRect;
        CellEdgeLength = CellData.EdgeLength;
        CellPaddingFactor = CellData.PaddingFactor;
    }
}
