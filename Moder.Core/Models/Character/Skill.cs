namespace Moder.Core.Models.Character;

public sealed class Skill
{
    public Skill(CharacterSkillType type, ushort maxValue)
    {
        Type = type;
        MaxValue = maxValue;
    }

    public CharacterSkillType Type { get; }
    public ushort MaxValue { get; }
}