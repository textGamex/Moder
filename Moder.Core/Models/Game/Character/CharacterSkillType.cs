using Ardalis.SmartEnum;

namespace Moder.Core.Models.Game.Character;

/// <summary>
/// 技能信息中的人物职业类型, 例如 Navy, CorpsCommander, FieldMarshal, Name 为 type 中可以使用的值
/// </summary>
public sealed class CharacterSkillType : SmartEnum<CharacterSkillType, byte>
{
    public static readonly CharacterSkillType Navy = new("navy", 0);
    public static readonly CharacterSkillType CorpsCommander = new("corps_commander", 1);
    public static readonly CharacterSkillType FieldMarshal = new("field_marshal", 2);

    public static CharacterSkillType FromCharacterType(string? characterType)
    {
        return characterType switch
        {
            "navy_leader" => Navy,
            "corps_commander" => CorpsCommander,
            "field_marshal" => FieldMarshal,
            _ => throw new ArgumentException($"未知的 character type: {characterType}"),
        };
    }

    private CharacterSkillType(string name, byte value)
        : base(name, value) { }
}
