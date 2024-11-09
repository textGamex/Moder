using Moder.Core.Models.Modifiers;

namespace Moder.Core.Models.Character;

/// <summary>
/// 人物特质
/// </summary>
public sealed class Trait
{
    public string Name { get; }
    public TraitType Type { get; }
    public IReadOnlyCollection<ModifierCollection> Modifiers { get; }
    
    public const string TraitSkillModifiersKey = "skill_modifiers_key";
    
    public Trait(string name, TraitType type, IEnumerable<ModifierCollection> modifiers)
    {
        Name = name;
        Type = type;
        Modifiers = modifiers.ToArray();
    }
}
