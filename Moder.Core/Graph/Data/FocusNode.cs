using Avalonia;
using Avalonia.Media;
using Moder.Core.Graph.Tools;

namespace Moder.Core.Graph.Data;

public class FocusNode : IRosterItem<PixelPoint>
{
    public PixelPoint Site { get; set; }

    public PixelPoint Signature => Site;
    public NodeType Type { get; set; }

    public static NodeColorSelector Colors { get; } = new();

    public Color Color => Colors[Type];
    public static FocusNode Default { get; } = new();

    public FocusNode(PixelPoint site, NodeType type)
    {
        Site = site;
        Type = type;
    }

    public FocusNode()
        : this(new PixelPoint(), NodeType.None) { }
}
