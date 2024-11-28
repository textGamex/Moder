namespace Moder.Core.Models;

public sealed class LanguageInfo(string displayName, string code)
{
    public string DisplayName { get; } = displayName;
    public string Code { get; } = code;

    public const string Default = "";
}
