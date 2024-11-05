namespace Moder.Core.Models.Modifiers;

public sealed class NodeModifier : IModifier
{
    public string Name { get; }
    public IReadOnlyCollection<Modifier> Modifiers { get; }
    public ModifierType Type => ModifierType.Node;

    public NodeModifier(string name, IEnumerable<Modifier> modifiers)
    {
        Name = name;
        Modifiers = modifiers.ToArray();
    }
}
