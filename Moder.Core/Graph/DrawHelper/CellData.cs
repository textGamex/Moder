using SkiaSharp;

namespace Moder.Core.Graph.DrawHelper;

public sealed class CellData
{
    private static int _edgeLength = 30;
    public static int EdgeLengthMin { get; set; } = 25;
    public static int EdgeLengthMax { get; set; } = 125;
    public static float CenterPaddingFactorMin { get; set; } = 0.01f;
    public static float CenterPaddingFactorMax { get; set; } = 0.4f;
    public static float PaddingFactor
    {
        get => _paddingFactor;
        set =>
            _paddingFactor =
                value < CenterPaddingFactorMin || value > CenterPaddingFactorMax ? _paddingFactor : value;
    }
    static float _paddingFactor = 0.2f;
    public static int EdgeLength
    {
        get => _edgeLength;
        set { _edgeLength = value < EdgeLengthMin || value > EdgeLengthMax ? _edgeLength : value; }
    }
    public static SKColor CellCenterShadeColor { get; set; } =
        new(SKColors.Orange.Red, SKColors.Orange.Green, SKColors.Orange.Blue, 125);
    public static SKColor CellAroundShadeColor { get; set; } = SKColors.DimGray;
}
