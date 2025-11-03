using Avalonia.Media;

namespace Moder.Core.Graph.Data;

public class NodeColorSelector
{
    public Color this[NodeType type] => ColorCollection[type];

    public void SetColor(NodeType type, string colorName)
    {
        if (Color.TryParse(colorName, out var color))
        {
            ColorCollection[type] = color;
        }
    }

    private Dictionary<NodeType, Color> ColorCollection { get; set; } =
        new() { [NodeType.None] = Colors.White, [NodeType.Normal] = Colors.DarkSlateGray };
}
