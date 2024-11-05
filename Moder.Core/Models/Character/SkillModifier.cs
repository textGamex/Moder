using Moder.Core.Models.Modifiers;
using ParadoxPower.Process;

namespace Moder.Core.Models.Character;

public sealed class SkillModifier
{
    public ushort Level { get; }
    public IReadOnlyList<Modifier> Modifiers { get; }
    
    public SkillModifier(ushort level, IEnumerable<Leaf> modifiers)
    {
        Level = level;
        Modifiers = modifiers.Select(Modifier.FromLeaf).ToArray();
    }
}