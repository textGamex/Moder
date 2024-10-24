namespace Moder.Core.Models.Character;

/// <summary>
/// 人物特质
/// </summary>
public sealed class Trait
{
    public string Name { get; }
    public TraitType Type { get; }
    
    public Trait(string name, TraitType type)
    {
        Name = name;
        Type = type;
    }
}