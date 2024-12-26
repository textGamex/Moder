namespace Moder.Core.Models.Game.Character;

/// <summary>
/// 存储某一项属性(攻击, 防御等)的每一级别的信息
/// </summary>
public sealed class SkillInfo
{
    public SkillType SkillType { get; }
    private readonly List<Skill> _skills = new(3);

    public SkillInfo(SkillType skillType)
    {
        SkillType = skillType;
    }

    public void Add(Skill skill)
    {
        _skills.Add(skill);
    }

    public ushort? GetMaxValue(SkillCharacterType type)
    {
        return _skills.Find(skill => skill.Type == type)?.MaxValue;
    }

    public SkillModifier GetModifierDescription(SkillCharacterType type, ushort level)
    {
        return _skills.Find(skill => skill.Type == type)?.GetModifier(level)
            ?? new SkillModifier(level, []);
    }
}
