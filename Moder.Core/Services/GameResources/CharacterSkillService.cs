using MethodTimer;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Moder.Core.Models;
using Moder.Core.Models.Character;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.Process;
using Colors = Microsoft.UI.Colors;

namespace Moder.Core.Services.GameResources;

/// <summary>
/// 用于提供人物技能的信息, 如最大值, 修正等
/// </summary>
public sealed class CharacterSkillService : CommonResourcesService<CharacterSkillService, List<SkillInfo>>
{
    private readonly ModifierService _modifierService;
    private const ushort DefaultSkillMaxValue = 1;

    private IEnumerable<SkillInfo> Skills => Resources.Values.SelectMany(s => s);

    [Time("加载人物技能信息")]
    public CharacterSkillService(ModifierService modifierService)
        : base(Path.Combine(Keywords.Common, "unit_leader"), WatcherFilter.Text)
    {
        _modifierService = modifierService;
    }

    public ushort GetMaxSkillValue(SkillType skillType, CharacterSkillType characterSkillType)
    {
        return Skills.FirstOrDefault(skill => skill.SkillType == skillType)?.GetMaxValue(characterSkillType)
            ?? DefaultSkillMaxValue;
    }

    public IEnumerable<Inline> GetSkillModifierDescription(
        SkillType skillType,
        CharacterSkillType characterSkillType,
        ushort level
    )
    {
        var skillModifier = Skills
            .FirstOrDefault(skill => skill.SkillType == skillType)
            ?.GetModifierDescription(characterSkillType, level);

        if (skillModifier is null || skillModifier.Modifiers.Count == 0)
        {
            return [new Run { Text = "无" }];
        }

        return _modifierService.GetModifierInlines(skillModifier.Modifiers);
    }

    protected override List<SkillInfo>? ParseFileToContent(Node rootNode)
    {
        var skills = new List<SkillInfo>();

        foreach (var node in rootNode.Nodes)
        {
            SkillType skillType;
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
            else
            {
                return null;
            }

            skills.Add(ParseSkills(node, skillType));
        }

        return skills;
    }

    private SkillInfo ParseSkills(Node node, SkillType skillType)
    {
        var skillMap = new Dictionary<CharacterSkillType, ushort>(3);
        var skillModifiers = new Dictionary<CharacterSkillType, List<SkillModifier>>(3);

        foreach (var skillInfoNode in node.Nodes)
        {
            var skillTypeLeaf = skillInfoNode.Leaves.FirstOrDefault(leaf =>
                StringComparer.OrdinalIgnoreCase.Equals(leaf.Key, "type")
            );
            if (skillTypeLeaf is null)
            {
                continue;
            }

            if (!CharacterSkillType.TryFromName(skillTypeLeaf.ValueText, true, out var type))
            {
                continue;
            }

            var currentSkillMaxValue = ushort.TryParse(skillInfoNode.Key, out var value)
                ? value
                : DefaultSkillMaxValue;
            var modifierNode = skillInfoNode.Nodes.FirstOrDefault(n =>
                StringComparer.OrdinalIgnoreCase.Equals(n.Key, "modifier")
            );
            if (skillMap.TryGetValue(type, out var maxValue))
            {
                skillMap[type] = Math.Max(maxValue, currentSkillMaxValue);
            }
            else
            {
                skillMap.Add(type, currentSkillMaxValue);
            }

            skillModifiers.TryGetValue(type, out var modifiers);
            if (modifiers is null)
            {
                modifiers = [];
                skillModifiers.Add(type, modifiers);
            }
            modifiers.Add(new SkillModifier(currentSkillMaxValue, modifierNode?.Leaves ?? []));
        }

        var skillInfo = new SkillInfo(skillType);
        foreach (var (type, maxValue) in skillMap)
        {
            skillInfo.Add(new Skill(type, maxValue, skillModifiers[type]));
        }

        return skillInfo;
    }
}
