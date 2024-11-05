namespace Moder.Core.Models.Modifiers;

public interface IModifier
{
    public string Name { get; }
    public ModifierType Type { get; }
}