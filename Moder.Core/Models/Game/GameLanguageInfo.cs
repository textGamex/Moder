namespace Moder.Core.Models.Game;

public sealed class GameLanguageInfo(string displayName, GameLanguage type)
{
    public string DisplayName { get; } = displayName;
    public GameLanguage Type { get; } = type;
}
