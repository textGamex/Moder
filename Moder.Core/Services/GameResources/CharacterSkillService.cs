using MethodTimer;
using Moder.Core.Models.Character;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.CSharp;
using ParadoxPower.Process;

namespace Moder.Core.Services.GameResources;

/// <summary>
/// 用于提供人物技能的信息, 如最大值, 修正等
/// </summary>
public sealed class CharacterSkillService : CommonResourcesService<CharacterSkillService, List<SkillInfo>>
{
    private const ushort DefaultSkillMaxValue = 1;

    private Dictionary<string, List<SkillInfo>>.ValueCollection Skills => Resources.Values;

    public CharacterSkillService()
        : base(Path.Combine(Keywords.Common, "unit_leader"), WatcherFilter.Text) { }

    public ushort GetMaxSkillValue(SkillType skillType, CharacterSkillType characterSkillType)
    {
        return Skills
                .SelectMany(s => s)
                .FirstOrDefault(skill => skill.SkillType == skillType)
                ?.GetMaxValue(characterSkillType) ?? DefaultSkillMaxValue;
    }

    protected override List<SkillInfo>? ParseFileToContent(Node rootNode)
    {
        var skills = new List<SkillInfo>();

        foreach (var node in rootNode.Nodes)
        {
            SkillType? skillType = null;
            if (StringComparer.OrdinalIgnoreCase.Equals(node.Key, "leader_attack_skills"))
            {
                skillType = SkillType.Attack;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals(node.Key, "leader_defense_skills"))
            {
                skillType = SkillType.Defense;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals(node.Key, "leader_skills"))
            {
                skillType = SkillType.Level;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals(node.Key, "leader_coordination_skills"))
            {
                skillType = SkillType.Coordination;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals(node.Key, "leader_logistics_skills"))
            {
                skillType = SkillType.Logistics;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals(node.Key, "leader_maneuvering_skills"))
            {
                skillType = SkillType.Maneuvering;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals(node.Key, "leader_planning_skills"))
            {
                skillType = SkillType.Planning;
            }

            if (skillType.HasValue)
            {
                skills.Add(ParseSkills(node, skillType.Value));
            }
        }

        return skills;
    }

    private SkillInfo ParseSkills(Node node, SkillType skillType)
    {
        var skillMap = new Dictionary<CharacterSkillType, ushort>(3);
        foreach (var skillInfoNode in node.Nodes)
        {
            var skillTypeLeaf = skillInfoNode.Leaves.FirstOrDefault(leaf =>
                StringComparer.OrdinalIgnoreCase.Equals(leaf.Key, "type")
            );
            if (skillTypeLeaf is not null)
            {
                if (!CharacterSkillType.TryFromName(skillTypeLeaf.ValueText, true, out var type))
                {
                    continue;
                }

                var currentSkillMaxValue = ushort.TryParse(skillInfoNode.Key, out var value)
                    ? value
                    : DefaultSkillMaxValue;
                if (skillMap.TryGetValue(type, out var maxValue))
                {
                    skillMap[type] = Math.Max(maxValue, currentSkillMaxValue);
                }
                else
                {
                    skillMap.Add(type, currentSkillMaxValue);
                }
            }
        }

        var skillInfo = new SkillInfo(skillType);
        foreach (var (type, maxValue) in skillMap)
        {
            skillInfo.Add(new Skill(type, maxValue));
        }

        return skillInfo;
    }
}
