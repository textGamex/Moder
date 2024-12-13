namespace Moder.Core.Models;

public sealed class CharacterTypeInfo(string displayName, string key)
{
    public string DisplayName { get; } = displayName;
    public string Key { get; } = key;
}