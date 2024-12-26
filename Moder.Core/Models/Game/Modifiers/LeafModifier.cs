using System.Diagnostics;
using Moder.Core.Extensions;
using ParadoxPower.Process;

namespace Moder.Core.Models.Game.Modifiers;

[DebuggerDisplay("{Key} = {Value}")]
public sealed class LeafModifier : IModifier, IEquatable<LeafModifier>
{
    public string Key { get; }
    /// <summary>
    /// 值, 当 <see cref="Key"/> 为 <see cref="CustomEffectTooltipKey"/> 时, Value 为本地化键
    /// </summary>
    public string Value { get; }
    public GameValueType ValueType { get; }
    public ModifierType Type => ModifierType.Leaf;

    public const string CustomEffectTooltipKey = "custom_effect_tooltip";
    public const string CustomModifierTooltipKey = "custom_modifier_tooltip";

    /// <summary>
    /// 从 <see cref="Leaf"/> 构建一个叶子修饰符
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

    public bool Equals(LeafModifier? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Key == other.Key && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is LeafModifier other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key, Value);
    }

    public static bool operator ==(LeafModifier? left, LeafModifier? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(LeafModifier? left, LeafModifier? right)
    {
        return !Equals(left, right);
    }
}
