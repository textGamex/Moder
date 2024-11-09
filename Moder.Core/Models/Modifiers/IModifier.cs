namespace Moder.Core.Models.Modifiers;

public interface IModifier
{
    public string Key { get; }
    public ModifierType Type { get; }
}