using System.Diagnostics;
using Moder.Core.Extensions;
using ParadoxPower.Process;

namespace Moder.Core.Models.Modifiers;

[DebuggerDisplay("{Key} = {Value}")]
public sealed class LeafModifier : IModifier
{
    public string Key { get; }
    public string Value { get; }
    public GameValueType ValueType { get; }
    public ModifierType Type => ModifierType.Leaf;

    /// <summary>
    /// 从 <see cref="Leaf"/> 构建一个叶子修饰符, <see cref="LocalizationKey"/> 属性被设置为 <c>leaf.Key</c>
    /// </summary>
    /// <param name="leaf">叶子</param>
    /// <returns></returns>
    public static LeafModifier FromLeaf(Leaf leaf) =>
        new(leaf.Key, leaf.ValueText, leaf.Value.ToLocalValueType());

    /// <summary>
    /// 构建一个叶子修饰符
    /// </summary>
    /// <param name="key">修饰符的键</param>
    /// <param name="value">值</param>
    /// <param name="valueType">值类型</param>
    public LeafModifier(string key, string value, GameValueType valueType)
    {
        Key = key;
        Value = value;
        ValueType = valueType;
    }
}
