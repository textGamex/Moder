using Avalonia;
using Avalonia.Skia;
using Moder.Core.Graph.Data;
using SkiaSharp;

namespace Moder.Core.Graph.DrawHelper;

public class GridTransform
{
    public static void ResetTransform()
    {
        GridData.Origin = new();
        GridData.PanoramaRect = new();
        GridData.DrawRect = new();
    }

    public static void OffsetOrigin(FocusTree focusTree, SKPoint offset)
    {
        GridData.PanoramaRect = SKRect.Create(
            -CellData.EdgeLength,
            -CellData.EdgeLength,
            focusTree.Width * CellData.EdgeLength,
            focusTree.Height * CellData.EdgeLength
        );
        var x = (GridData.Origin.X + offset.X) % GridData.PanoramaRect.Width;
        if (x < 0)
        {
            x += GridData.PanoramaRect.Width;
        }
        var y = (GridData.Origin.Y + offset.Y) % GridData.PanoramaRect.Height;
        if (y < 0)
        {
            y += GridData.PanoramaRect.Height;
        }
        GridData.Origin = new(x, y);
        GridData.RedrawAll = true;
    }

    public static void SetSelectPoint(FocusTree focusTree, Point? point)
    {
        if (point is null)
        {
            GridData.SelectSite = new(-1, -1);
            GridData.SelectCellPart = Direction.None;
        }
        else
        {
            var cell = new Cell();
            var pos = point.Value.ToSKPoint();
            cell.SetRealPoint(focusTree, pos);
            GridData.SelectSite = cell.Site;
            GridData.SelectCellPart = cell.GetRealPointOnPart(pos);
        }
    }

    public static void CheckFocusPoint(FocusTree focusTree, Point point)
    {
        var cell = new Cell();
        cell.SetRealPoint(focusTree, point.ToSKPoint());
        GridData.FocusSite = GridData.FocusSite == cell.Site ? null : cell.Site;
    }
}
