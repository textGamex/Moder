using ParadoxPower.Process;

namespace Moder.Core.Models.Character;

public sealed class SkillModifier
{
    public ushort Level { get; }
    public IReadOnlyList<Modifier> Modifiers { get; }
    
    public SkillModifier(ushort level, IEnumerable<Leaf> modifiers)
    {
        Level = level;
        Modifiers = modifiers.Select(m => new Modifier(m)).ToArray();
    }
}