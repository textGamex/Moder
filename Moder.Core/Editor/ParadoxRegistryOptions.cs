using Avalonia.Styling;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace Moder.Core.Editor;

public sealed class ParadoxRegistryOptions(ThemeVariant? _theme) : IRegistryOptions
{
    private static string ThemesFolderPath => Path.Combine(App.AssetsFolder, "CodeEditor", "Themes");
    private static string GrammarsFolderPath => Path.Combine(App.AssetsFolder, "CodeEditor", "Grammars");

    public IRawTheme GetTheme(string scopeName)
    {
        if (string.IsNullOrWhiteSpace(scopeName))
        {
            return GetDefaultTheme();
        }

        var path = Path.Combine(ThemesFolderPath, scopeName);
        if (!File.Exists(path))
        {
            return GetDefaultTheme();
        }

        return ThemeReader.ReadThemeSync(File.OpenText(path));
    }

    public IRawGrammar GetGrammar(string scopeName)
    {
        return GrammarReader.ReadGrammarSync(
            File.OpenText(Path.Combine(GrammarsFolderPath, "paradox.tmLanguage.json"))
        );
    }

    public ICollection<string>? GetInjections(string scopeName)
    {
        return null;
    }

    public IRawTheme GetDefaultTheme()
    {
        return ThemeReader.ReadThemeSync(File.OpenText(Path.Combine(ThemesFolderPath, GetThemeFileName(_theme))));
    }
    
    public IRawTheme LoadTheme(ThemeVariant theme)
    {
        return ThemeReader.ReadThemeSync(File.OpenText(Path.Combine(ThemesFolderPath, GetThemeFileName(theme))));;
    }

    private static string GetThemeFileName(ThemeVariant? theme)
    {
        if (theme == ThemeVariant.Dark)
        {
            return "dark_plus.json";
        }

        if (theme == ThemeVariant.Light)
        {
            return "light_plus.json";
        }

        return "dark_plus.json";
    }
}
