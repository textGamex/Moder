namespace Moder.Core.Models.Game.Modifiers;

public interface IModifier
{
    public string Key { get; }
    public ModifierType Type { get; }
}