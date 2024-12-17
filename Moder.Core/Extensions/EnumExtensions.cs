using System.Globalization;
using Avalonia.Styling;
using EnumsNET;
using Moder.Core.Models;
using Moder.Core.Models.Game;

namespace Moder.Core.Extensions;

public static class EnumExtensions
{
    public static object? ToEnum(this string str, Type enumType)
    {
        try
        {
            if (Enums.TryParse(enumType, str, true, out var result))
            {
                return result;
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return null;
    }
    
    public static string ToGameLocalizationLanguage(this GameLanguage language)
    {
        if (language == GameLanguage.Default)
        {
            language = GetSystemLanguage();
        }

        return language switch
        {
            GameLanguage.Chinese => "simp_chinese",
            GameLanguage.English => "english",
            GameLanguage.Russian => "russian",
            GameLanguage.Spanish => "spanish",
            GameLanguage.German => "german",
            GameLanguage.French => "french",
            GameLanguage.Japanese => "japanese",
            GameLanguage.Portuguese => "braz_por",
            GameLanguage.Polish => "polish",
            GameLanguage.Default => throw new ArgumentException($"{nameof(GameLanguage.Default)} 未转换为系统本地语言"),
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };
    }
    
    private static GameLanguage GetSystemLanguage()
    {
        var cultureInfo = CultureInfo.CurrentUICulture;
        var name = cultureInfo.Name;

        if (name.StartsWith("zh"))
        {
            return GameLanguage.Chinese;
        }
        if (name.StartsWith("es"))
        {
            return GameLanguage.Spanish;
        }
        if (name.StartsWith("de"))
        {
            return GameLanguage.German;
        }
        if (name.StartsWith("ja"))
        {
            return GameLanguage.Japanese;
        }
        if (name.StartsWith("fr"))
        {
            return GameLanguage.French;
        }
        if (name.StartsWith("ru"))
        {
            return GameLanguage.Russian;
        }
        if (name.Contains("pt-BR"))
        {
            return GameLanguage.Portuguese;
        }
        if (name.StartsWith("pl"))
        {
            return GameLanguage.Polish;
        }

        return GameLanguage.English;
    }
    
    public static ThemeVariant ToThemeVariant(this ThemeMode type)
    {
        return type switch
        {
            ThemeMode.Light => new ThemeVariant(nameof(ThemeMode.Light), ThemeVariant.Light),
            ThemeMode.Dark => new ThemeVariant(nameof(ThemeMode.Dark), ThemeVariant.Dark),
            ThemeMode.DarkSlateGray => new ThemeVariant(nameof(ThemeMode.DarkSlateGray), ThemeVariant.Dark),
            _ => ThemeVariant.Default,
        };
    }
}
