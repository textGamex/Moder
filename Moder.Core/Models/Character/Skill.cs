namespace Moder.Core.Models.Character;

/// <summary>
/// 技能的相关信息
/// </summary>
public sealed class Skill
{
    public CharacterSkillType Type { get; }
    public ushort MaxValue { get; }
    
    private readonly SkillModifier[] _modifiers;

    public Skill(CharacterSkillType type, ushort maxValue, IEnumerable<SkillModifier> modifiers)
    {
        Type = type;
        MaxValue = maxValue;
        _modifiers = modifiers.ToArray();
    }

    public SkillModifier GetModifier(ushort level)
    {
        return Array.Find(_modifiers, x => x.Level == level) ?? new SkillModifier(level, []);
    }
}