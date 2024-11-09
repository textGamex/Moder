using Moder.Core.Extensions;
using ParadoxPower.Process;

namespace Moder.Core.Models.Modifiers;

public sealed class LeafModifier : IModifier
{
    public string Key { get; }
    public string Value { get; }
    public GameValueType ValueType { get; }
    public ModifierType Type => ModifierType.Leaf;

    private LeafModifier(Leaf leaf)
        : this(leaf.Key, leaf.ValueText, leaf.Value.ToLocalValueType()) { }

    public static LeafModifier FromLeaf(Leaf leaf) => new(leaf);

    public LeafModifier(string key, string value, GameValueType valueType)
    {
        Key = key;
        Value = value;
        ValueType = valueType;
    }
}
