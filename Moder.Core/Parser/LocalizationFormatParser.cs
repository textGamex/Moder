using System.Diagnostics.CodeAnalysis;
using Pidgin;
using static Pidgin.Parser;

namespace Moder.Core.Parser;

public static class LocalizationFormatParser
{
    private static readonly Parser<char, char> CharExcept = Parser<char>.Token(c => c != '$');

    private static readonly Parser<char, LocalizationFormat> PlaceholderParser = Char('$')
        .Then(CharExcept.AtLeastOnceString())
        .Before(Char('$'))
        .Map(placeholder => new LocalizationFormat(placeholder, LocalizationFormatType.Placeholder));

    private static readonly Parser<char, LocalizationFormat> TextWithColorParser = 
        Parser<char>.Token(c => c != '§').AtLeastOnceString().Optional()
        .Between(Char('§'), String("§!"))
        .Map(text => new LocalizationFormat(
            text.HasValue ? text.Value : string.Empty,
            LocalizationFormatType.TextWithColor
        ));

    private static readonly Parser<char, LocalizationFormat> TextParser =
        from text in Try(String("$$").WithResult('$')).Or(AnyCharExcept('$', '§')).AtLeastOnceString()
        select new LocalizationFormat(text, LocalizationFormatType.Text);

    private static readonly Parser<char, IEnumerable<LocalizationFormat>> LocalizationTextParser = TextParser
        .Or(PlaceholderParser)
        .Or(TextWithColorParser)
        .Many();

    public static bool TryParse(string input, [NotNullWhen(true)] out IEnumerable<LocalizationFormat>? result)
    {
        var parseResult = LocalizationTextParser.Parse(input);
        result = parseResult.Success ? parseResult.Value : null;

        return parseResult.Success;
    }
}
