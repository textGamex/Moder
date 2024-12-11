namespace Moder.Core.Infrastructure;

public interface ITabViewItem
{
    public string Header { get; }

    /// <summary>
    /// 唯一识别字符串, 用来在 TabView 中判断是否存在
    /// </summary>
    public string Id { get; }
    public string ToolTip { get; }
    public bool Equals(ITabViewItem? other)
    {
        return Id == other?.Id;
    }
}
