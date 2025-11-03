using Avalonia;
using Moder.Core.Graph.Data;
using SkiaSharp;

namespace Moder.Core.Graph.DrawHelper;

public sealed class Cell
{
    public PixelPoint Site { get; private set; }
    public FocusNode FocusNode { get; private set; } = FocusNode.Default;
    int EdgeLength { get; set; }
    float CenterPadding { get; set; }
    float CenterLength { get; set; }
    float CenterAddOnePadding { get; set; }
    SKPoint GridOrigin { get; set; }
    SKRect DrawRect { get; set; }
    SKRect PanoramaRect { get; set; }

    public Cell()
        : this(
            CellData.EdgeLength,
            CellData.PaddingFactor,
            GridData.Origin,
            GridData.DrawRect,
            GridData.PanoramaRect
        ) { }

    public Cell(GridDrawData drawData)
        : this(
            drawData.CellEdgeLength,
            drawData.CellPaddingFactor,
            drawData.Origin,
            drawData.DrawRect,
            drawData.PanoramaRect
        ) { }

    public Cell(int egdeLength, float paddingFactor, SKPoint gridOrigin, SKRect drawRect, SKRect panoramaRect)
    {
        SetEgdeLength(egdeLength, paddingFactor);
        GridOrigin = gridOrigin;
        DrawRect = drawRect;
        PanoramaRect = panoramaRect;
    }

    public void SetEgdeLength(int egdeLength, float paddingFactor)
    {
        EdgeLength = egdeLength;
        CenterPadding = EdgeLength * paddingFactor;
        CenterLength = EdgeLength - CenterPadding * 2;
        CenterAddOnePadding = CenterLength + CenterPadding;
    }

    public void SetGridSite(FocusTree focusTree, PixelPoint site)
    {
        Site = focusTree.SetPointWithin(site);
        FocusNode = focusTree[Site];
    }

    public void SetRealPoint(FocusTree focusTree, SKPoint realPoint)
    {
        Site = RealPointToSite(focusTree, realPoint, EdgeLength, GridOrigin);
        FocusNode = focusTree[Site];
    }

    private static PixelPoint RealPointToSite(
        FocusTree focusTree,
        SKPoint realPoint,
        int edgeLength,
        SKPoint origin
    )
    {
        var dX = realPoint.X - origin.X;
        var x = dX / edgeLength;
        if (dX < 0)
        {
            x += focusTree.Width;
        }
        var dY = realPoint.Y - origin.Y;
        var y = dY / edgeLength;
        if (dY < 0)
        {
            y += focusTree.Height;
        }
        return new((int)x, (int)y);
    }

    private (float, float) GridPointToRealLeftTop()
    {
        var x = EdgeLength * Site.X + GridOrigin.X;
        if (x < PanoramaRect.Left)
        {
            x += PanoramaRect.Width;
        }
        else if (x > PanoramaRect.Right)
        {
            x -= PanoramaRect.Width;
        }
        var y = EdgeLength * Site.Y + GridOrigin.Y;
        if (y < PanoramaRect.Top)
        {
            y += PanoramaRect.Height;
        }
        else if (y > PanoramaRect.Bottom)
        {
            y -= PanoramaRect.Height;
        }
        return new(x, y);
    }

    public SKRect GetBounds()
    {
        var (left, top) = GridPointToRealLeftTop();
        var bounds = SKRect.Create(left, top, EdgeLength, EdgeLength);
        return SKRect.Intersect(bounds, DrawRect);
    }

    public SKRect GetPartBounds(Direction part)
    {
        var (left, top) = GridPointToRealLeftTop();
        var center = SKRect.Create(left + CenterPadding, top + CenterPadding, CenterLength, CenterLength);
        var bounds = part switch
        {
            Direction.Center => center,
            Direction.Left => SKRect.Create(left, center.Top, CenterPadding, CenterLength),
            Direction.Top => SKRect.Create(center.Left, top, CenterLength, CenterPadding),
            Direction.Right => SKRect.Create(center.Right, center.Top, CenterPadding, center.Height),
            Direction.Bottom => SKRect.Create(center.Left, center.Bottom, center.Width, CenterPadding),
            Direction.LeftTop => SKRect.Create(left, top, CenterPadding, CenterPadding),
            Direction.TopRight => SKRect.Create(center.Right, top, CenterPadding, CenterPadding),
            Direction.LeftBottom => SKRect.Create(left, center.Bottom, CenterPadding, CenterPadding),
            Direction.BottomRight => SKRect.Create(center.Right, center.Bottom, CenterPadding, CenterPadding),
            _ => default,
        };
        return SKRect.Intersect(bounds, DrawRect);
    }

    public SKRect GetBoundsInDirection(Direction direction)
    {
        var (left, top) = GridPointToRealLeftTop();
        var bounds = SKRect.Create(left, top, EdgeLength, EdgeLength);
        var center = SKRect.Create(left + CenterPadding, top + CenterPadding, CenterLength, CenterLength);
        bounds = direction switch
        {
            Direction.Center => bounds,
            Direction.Left => SKRect.Create(center.Left, bounds.Top, CenterAddOnePadding, EdgeLength),
            Direction.Top => SKRect.Create(bounds.Left, center.Top, EdgeLength, CenterAddOnePadding),
            Direction.Right => SKRect.Create(bounds.Left, bounds.Top, CenterAddOnePadding, EdgeLength),
            Direction.Bottom => SKRect.Create(bounds.Left, bounds.Top, EdgeLength, CenterAddOnePadding),
            Direction.LeftTop => SKRect.Create(
                center.Left,
                center.Top,
                CenterAddOnePadding,
                CenterAddOnePadding
            ),
            Direction.TopRight => SKRect.Create(
                bounds.Left,
                center.Top,
                CenterAddOnePadding,
                CenterAddOnePadding
            ),
            Direction.LeftBottom => SKRect.Create(
                center.Left,
                bounds.Top,
                CenterAddOnePadding,
                CenterAddOnePadding
            ),
            Direction.BottomRight => SKRect.Create(
                bounds.Left,
                bounds.Top,
                CenterAddOnePadding,
                CenterAddOnePadding
            ),
            _ => default,
        };
        return SKRect.Intersect(bounds, DrawRect);
    }

    public Direction GetRealPointOnPart(SKPoint realPoint)
    {
        if (GetPartBounds(Direction.Center).Contains(realPoint))
        {
            return Direction.Center;
        }
        if (GetPartBounds(Direction.Left).Contains(realPoint))
        {
            return Direction.Left;
        }
        if (GetPartBounds(Direction.Top).Contains(realPoint))
        {
            return Direction.Top;
        }
        if (GetPartBounds(Direction.Right).Contains(realPoint))
        {
            return Direction.Right;
        }
        if (GetPartBounds(Direction.Bottom).Contains(realPoint))
        {
            return Direction.Bottom;
        }
        if (GetPartBounds(Direction.LeftTop).Contains(realPoint))
        {
            return Direction.LeftTop;
        }
        if (GetPartBounds(Direction.TopRight).Contains(realPoint))
        {
            return Direction.TopRight;
        }
        if (GetPartBounds(Direction.BottomRight).Contains(realPoint))
        {
            return Direction.BottomRight;
        }
        if (GetPartBounds(Direction.LeftBottom).Contains(realPoint))
        {
            return Direction.LeftBottom;
        }
        return Direction.None;
    }

    public static SKColor GetPartShadeColor(Direction part)
    {
        return part switch
        {
            Direction.Center => CellData.CellCenterShadeColor,
            _ => CellData.CellAroundShadeColor,
        };
    }

    public override string ToString()
    {
        return $"site:{Site} land-type:{FocusNode.Type}";
    }
}
