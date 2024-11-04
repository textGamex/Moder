using Moder.Core.Extensions;
using ParadoxPower.Process;

namespace Moder.Core.Models;

public sealed class Modifier
{
    public string Name { get; }
    public string Value { get; }
    public GameValueType ValueType { get; }

    public Modifier(Leaf leaf)
    {
        Name = leaf.Key;
        Value = leaf.ValueText;
        ValueType = leaf.Value.ToLocalValueType();
    }
}