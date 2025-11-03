using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Moder.Core.Infrastructure;
using Moder.Language.Strings;

namespace Moder.Core.Views.Game;

public partial class FocusTreeControlView : UserControl, ITabViewItem
{
    public string Header { get; } = Resource.Menu_FocusTree;
    public string Id { get; } = nameof(FocusTreeControlView);
    public string ToolTip => Header;

    public FocusTreeControlView()
    {
        InitializeComponent();
    }
}
