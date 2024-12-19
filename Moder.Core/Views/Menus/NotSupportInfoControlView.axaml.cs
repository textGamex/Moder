using Avalonia.Controls;
using Moder.Core.Infrastructure;
using Moder.Core.Models;

namespace Moder.Core.Views.Menus;

public sealed partial class NotSupportInfoControlView : UserControl, ITabViewItem
{
    public NotSupportInfoControlView(SystemFileItem item)
    {
        InitializeComponent();
        Header = item.Name;
        Id = item.FullPath;
        ToolTip = item.FullPath;
    }

    public string Header { get; }
    public string Id { get; }
    public string ToolTip { get; }
}
