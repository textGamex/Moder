namespace Moder.Core.Infrastructure.Parser;

public sealed class LocalizationFormatInfo(string text, LocalizationFormatType type)
{
    public string Text => text;
    public LocalizationFormatType Type => type;
}