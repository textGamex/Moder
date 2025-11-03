using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Moder.Core.Graph.Data;
using Moder.Core.Graph.DrawHelper;
using Moder.Core.Graph.Tools;
using Moder.Core.Infrastructure;
using Moder.Language.Strings;

namespace Moder.Core.Views.Game;

public partial class FocusTreeEditorControlView : UserControl, ITabViewItem, IClosed
{
    public string Header { get; } = Resource.Menu_FocusTree;
    public string Id { get; } = nameof(FocusTreeEditorControlView);
    public string ToolTip => Header;

    public FocusTreeEditorControlView()
    {
        InitializeComponent();

        // TODO: for test
        var focusNode = new Roster<PixelPoint, FocusNode>();
        focusNode.TryAdd(new FocusNode(new PixelPoint(2, 2), NodeType.Normal));
        focusNode.TryAdd(new FocusNode(new PixelPoint(6, 5), NodeType.Normal));
        var focusTree = new FocusTree { Size = new PixelSize(50, 50), FocusNodes = focusNode };
        FocusTreeImage.FocusTree = focusTree;

        // TODO: Bug
        MinWidth = 500;
        MinHeight = 500;
    }

    public void Close()
    {
        // TODO
    }
}
