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

    public ushort? GetMaxValue(CharacterSkillType skillType)
    {
        return _skills.Find(skill => skill.Type == skillType)?.MaxValue;
    }

    public SkillModifier GetModifierDescription(CharacterSkillType skillType, ushort level)
    {
        return _skills.Find(skill => skill.Type == skillType)?.GetModifier(level)
            ?? new SkillModifier(level, []);
    }
}
