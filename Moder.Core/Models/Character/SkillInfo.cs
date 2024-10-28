namespace Moder.Core.Models.Character;

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
        return _skills.FirstOrDefault(s => s.Type == skillType)?.MaxValue;
    }
}