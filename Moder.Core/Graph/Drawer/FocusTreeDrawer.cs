using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Graph.Data;
using Moder.Core.Graph.DrawHelper;
using NLog;
using SkiaSharp;

namespace Moder.Core.Graph.Drawer;

public class FocusTreeDrawer : ICustomDrawOperation
{
    public Rect Bounds { get; }
    private SKColor ForeColor { get; }
    private SKColor BackColor { get; }
    private FocusTree FocusTree { get; }

    public bool Equals(ICustomDrawOperation? other)
    {
        return false;
    }

    public void Dispose() { }

    public bool HitTest(Point p)
    {
        return true;
    }

    public FocusTreeDrawer(Rect bounds, SKColor foreColor, SKColor backColor, FocusTree focusTree)
    {
        Bounds = bounds;
        ForeColor = foreColor;
        BackColor = backColor;
        FocusTree = focusTree;
    }

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Error("Current rendering API is not Skia");
            return;
        }
        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;
        GridDraw.Draw(FocusTree, canvas, Bounds, ForeColor, BackColor);
    }
}
