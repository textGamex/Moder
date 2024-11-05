namespace Moder.Core.Models.Modifiers;

/// <summary>
/// Modifier 集合, 只有需要 Modifier 节点的 Key 时才会使用到这个类
/// </summary>
public sealed class ModifierCollection
{
    /// <summary>
    /// 容纳 Modifier 的 Key
    /// </summary>
    public string Key { get; }
    public IEnumerable<IModifier> Modifiers { get; }

    public ModifierCollection(string key, IEnumerable<IModifier> modifiers)
    {
        Key = key;
        Modifiers = modifiers;
    }
}
