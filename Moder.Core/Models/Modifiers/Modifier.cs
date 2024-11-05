using Moder.Core.Extensions;
using ParadoxPower.Process;

namespace Moder.Core.Models.Modifiers;

public sealed class Modifier : IModifier
{
    public string Name { get; }
    public string Value { get; }
    public GameValueType ValueType { get; }
    public ModifierType Type => ModifierType.Leaf;

    private Modifier(Leaf leaf)
        : this(leaf.Key, leaf.ValueText, leaf.Value.ToLocalValueType()) { }

    public static Modifier FromLeaf(Leaf leaf) => new(leaf);

    public Modifier(string name, string value, GameValueType valueType)
    {
        Name = name;
        Value = value;
        ValueType = valueType;
    }
}
