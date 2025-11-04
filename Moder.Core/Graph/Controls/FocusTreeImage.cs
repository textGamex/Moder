using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Skia;
using Avalonia.Threading;
using Moder.Core.Graph.Data;
using Moder.Core.Graph.Drawer;
using Moder.Core.Graph.DrawHelper;
using SkiaSharp;

namespace Moder.Core.Graph.Controls;

public class FocusTreeImage : Control
{
    public FocusTree? FocusTree { get; set; }
    private bool DoDragGraph { get; set; }
    private Point DragStartPoint { get; set; }
    private SKColor SkForeColor { get; set; } = SKColors.White;
    private SKColor SkBackColor { get; set; } = SKColors.SkyBlue;

    public bool ShowEmptyGrid
    {
        get => GetValue(ShowEmptyGridProperty);
        set => SetValue(ShowEmptyGridProperty, value);
    }
    public static readonly StyledProperty<bool> ShowEmptyGridProperty = AvaloniaProperty.Register<
        FocusTreeImage,
        bool
    >(nameof(ShowEmptyGrid), false, false, BindingMode.TwoWay);

    public Color ForeColor
    {
        get => GetValue(ForeColorProperty);
        set => SetValue(ForeColorProperty, value);
    }
    public static readonly StyledProperty<Color> ForeColorProperty = AvaloniaProperty.Register<
        FocusTreeImage,
        Color
    >(nameof(ForeColor), Colors.White);

    public Color BackColor
    {
        get => GetValue(BackColorProperty);
        set => SetValue(BackColorProperty, value);
    }
    public static readonly StyledProperty<Color> BackColorProperty = AvaloniaProperty.Register<
        FocusTreeImage,
        Color
    >(nameof(BackColor), Colors.SkyBlue);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        DoDragGraph = true;
        var position = e.GetPosition(this);
        DragStartPoint = position;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        DoDragGraph = false;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (FocusTree != null)
        {
            GridTransform.SetSelectPoint(FocusTree, null);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (FocusTree == null)
        {
            return;
        }
        var position = e.GetPosition(this);
        GridTransform.SetSelectPoint(FocusTree, position);
        if (DoDragGraph)
        {
            var offset = position - DragStartPoint;
            DragStartPoint = position;
            GridTransform.OffsetOrigin(FocusTree, offset.ToSKPoint());
        }
        else
        {
            // GridData.SelectPoint = position.ToSKPoint();
            // TODO: for test
            var cell = new Cell();
            cell.SetRealPoint(FocusTree, position.ToSKPoint());
            Debug.WriteLine(cell);
            // InvalidateVisual();
            // GridDrawer.DrawSelect(Bounds.Size, BackColor, e.Location);
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (FocusTree == null)
        {
            return;
        }
        var position = e.GetPosition(this);
        var rect = new Rect(Bounds.Size);
        var diffInWidth = position.X - rect.Width / 2;
        var diffInHeight = position.Y - rect.Height / 2;
        var dX = diffInWidth / CellData.EdgeLength * rect.Width / 200;
        var dY = diffInHeight / CellData.EdgeLength * rect.Height / 200;
        CellData.EdgeLength += (int)Math.Round(e.Delta.Y / 10 * Math.Max(rect.Width, rect.Height) / 200);
        GridTransform.OffsetOrigin(FocusTree, new SKPoint((float)dX, (float)dY));
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (FocusTree is null)
        {
            return;
        }
        using var drawer = new FocusTreeDrawer(
            new Rect(Bounds.Size),
            ForeColor.ToSKColor(),
            BackColor.ToSKColor(),
            FocusTree
        );
        context.Custom(drawer);
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }
}
