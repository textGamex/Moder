namespace Moder.Core.Models;

public sealed class CharacterTypeInfo(string displayName, string keyword)
{
    /// <summary>
    /// 显示在 UI 上的名称
    /// </summary>
    public string DisplayName { get; } = displayName;
    /// <summary>
    /// 在代码中的关键字
    /// </summary>
    public string Keyword { get; } = keyword;
}