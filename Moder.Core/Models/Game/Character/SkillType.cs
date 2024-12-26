namespace Moder.Core.Models.Game.Character;

/// <summary>
/// 技能类型, 比如攻击, 防御等等
/// </summary>
public enum SkillType : byte
{
    Level,
    Attack,
    Defense,
    Planning,
    Logistics,
    Maneuvering,
    Coordination
}