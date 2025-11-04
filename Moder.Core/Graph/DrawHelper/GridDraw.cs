using System.Diagnostics;
using Avalonia;
using Avalonia.Skia;
using FSharp.Data.Runtime;
using HarfBuzzSharp;
using Moder.Core.Graph.Data;
using SkiaSharp;

namespace Moder.Core.Graph.DrawHelper;

public sealed class GridDraw
{
    private static GridDrawData? DrawData { get; set; }

    public static void Draw(
        FocusTree focusTree,
        SKCanvas canvas,
        Rect bounds,
        SKColor foreColor,
        SKColor backColor
    )
    {
        var width = (int)bounds.Width;
        var height = (int)bounds.Height;
        if (width is 0 || height is 0)
        {
            return;
        }
        GridData.DrawRect = SKRect.Create(0, 0, width, height);
        using var paint = new SKPaint();
        DrawData = new GridDrawData(foreColor, backColor);
        Draw(focusTree, canvas, paint, DrawData);
    }

    private static void Draw(FocusTree focusTree, SKCanvas canvas, SKPaint paint, GridDrawData drawData)
    {
        DrawGrid(focusTree, canvas, paint, drawData);
        DrawCrossLine(canvas, paint, drawData);
    }

    private static void DrawGrid(FocusTree focusTree, SKCanvas canvas, SKPaint paint, GridDrawData drawData)
    {
        canvas.Clear(drawData.BackColor);
        var width = (int)(drawData.DrawRect.Width / drawData.CellEdgeLength + 2);
        var height = (int)(drawData.DrawRect.Height / drawData.CellEdgeLength + 2);
        var offsetX = (int)(drawData.Origin.X / drawData.CellEdgeLength + 1);
        var offsetY = (int)(drawData.Origin.Y / drawData.CellEdgeLength + 1);
        var cell = new Cell(drawData);
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var site = new PixelPoint(i - offsetX, j - offsetY);
                cell.SetGridSite(focusTree, site);
                if (GridData.FocusSite == cell.Site)
                {
                    DrawFocusCell(canvas, paint, cell);
                }
                else if (GridData.SelectSite == cell.Site)
                {
                    DrawSelecteCell(canvas, paint, cell);
                }
                else
                {
                    DrawCell(canvas, paint, cell, drawData.ForeColor);
                }
            }
        }
    }

    public static void DrawSelecteCell(SKCanvas canvas, SKPaint paint, Cell cell)
    {
        var bounds = cell.GetPartBounds(GridData.SelectCellPart);
        paint.Color = Cell.GetPartShadeColor(GridData.SelectCellPart);
        canvas.DrawRect(bounds, paint);
    }

    public static void DrawFocusCell(SKCanvas canvas, SKPaint paint, Cell cell)
    {
        var bounds = cell.GetPartBounds(Direction.Center);
        paint.Color = GridData.FocusColor;
        canvas.DrawRect(bounds, paint);
    }

    private static void DrawCell(SKCanvas canvas, SKPaint paint, Cell cell, SKColor foreColor)
    {
        //TODO: for test
        if (cell.FocusNode.Type != NodeType.None)
        {
            var rect = cell.GetPartBounds(Direction.Center);
            var x = rect.Left;
            var y = rect.Top;
            var width = rect.Width;
            var height = rect.Height;
            rect = SKRect.Create(x, y, width, height);
            paint.Color = cell.FocusNode.Color.ToSKColor();
            canvas.DrawRect(rect, paint);
        }
        else if (GridData.ShowEmptyGrid)
        {
            paint.Color = foreColor;
            paint.IsStroke = true;
            canvas.DrawRect(cell.GetPartBounds(Direction.Center), paint);
            paint.IsStroke = false;
        }
    }

    private static void DrawCrossLine(SKCanvas canvas, SKPaint paint, GridDrawData drawData)
    {
        paint.Color = GridData.GuideLineColor;
        paint.StrokeWidth = GridData.GuideLineThickness;
        canvas.DrawLine(
            new(drawData.Origin.X, drawData.DrawRect.Top),
            new(drawData.Origin.X, drawData.DrawRect.Bottom),
            paint
        );
        canvas.DrawLine(
            new(drawData.DrawRect.Left, drawData.Origin.Y),
            new(drawData.DrawRect.Right, drawData.Origin.Y),
            paint
        );
    }
}
