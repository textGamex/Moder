using FluentAvalonia.UI.Controls;

namespace Moder.Core.Infrastructure;

public interface ITabViewItem
{
    /// <summary>
    /// 选项卡显示的名称
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// 唯一识别字符串, 用来在 TabView 中判断是否存在
    /// </summary>
    public string Id { get; }
    public string ToolTip { get; }

    /// <summary>
    /// 选项卡左侧显示的图标
    /// </summary>
    public IconSource? Icon => null;

    public bool Equals(ITabViewItem? other)
    {
        return Id == other?.Id;
    }
}
