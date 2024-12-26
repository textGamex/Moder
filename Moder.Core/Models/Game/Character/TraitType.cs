namespace Moder.Core.Models.Game.Character;

[Flags]
public enum TraitType : byte
{
    None = 0,
    Land = 1,
    Navy = 2,
    CorpsCommander = 4,
    FieldMarshal = 8,
    /// <summary>
    /// 特工
    /// </summary>
    Operative = 16,
    All = Land | Navy | CorpsCommander | FieldMarshal | Operative
}