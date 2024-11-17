namespace Moder.Core.Parser;

public sealed class LocalizationFormat(string text, LocalizationFormatType type)
{
    public string Text => text;
    public LocalizationFormatType Type => type;
}