namespace Moder.Core.Models.Modifiers;

public sealed class NodeModifier : IModifier
{
    public string Key { get; }
    public IReadOnlyList<LeafModifier> Modifiers { get; }
    public ModifierType Type => ModifierType.Node;

    public NodeModifier(string key, IEnumerable<LeafModifier> modifiers)
    {
        Key = key;
        Modifiers = modifiers.ToArray();
    }
}
