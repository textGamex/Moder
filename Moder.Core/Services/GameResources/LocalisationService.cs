using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Extensions;
using Moder.Core.Services.Config;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.CSharp;
using ParadoxPower.Localisation;

namespace Moder.Core.Services.GameResources;

public sealed class LocalisationService
    : ResourcesService<LocalisationService, FrozenDictionary<string, string>, YAMLLocalisationParser.LocFile>
{
    private Dictionary<string, FrozenDictionary<string, string>>.ValueCollection Localisations =>
        Resources.Values;

    public LocalisationService()
        : base(
            Path.Combine(
                "localisation",
                App.Current.Services.GetRequiredService<GlobalSettingService>()
                    .GameLanguage.ToGameLocalizationLanguage()
            ),
            WatcherFilter.LocalizationFiles
        ) { }

    /// <summary>
    /// 如果本地化文本不存在, 则返回<c>key</c>
    /// </summary>
    /// <returns></returns>
    public string GetValue(string key)
    {
        foreach (var localisation in Localisations)
        {
            if (localisation.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        return key;
    }

    protected override FrozenDictionary<string, string> ParseFileToContent(
        YAMLLocalisationParser.LocFile result
    )
    {
        var localisations = new Dictionary<string, string>(result.entries.Length);
        foreach (var item in result.entries)
        {
            localisations[item.key] = GetCleanDesc(item.desc);
        }

        return localisations.ToFrozenDictionary();
    }

    /// 去除开头和结尾的 "
    private static string GetCleanDesc(string rawDesc)
    {
        return rawDesc.Length switch
        {
            > 2 => rawDesc[1..^1],
            2 => string.Empty,
            _ => rawDesc
        };
    }

    protected override YAMLLocalisationParser.LocFile? GetParseResult(string filePath)
    {
        var localisation = YAMLLocalisationParser.parseLocFile(filePath);
        if (localisation.IsFailure)
        {
            Logger.LogParseError(localisation.GetError());
            return null;
        }

        var result = localisation.GetResult();
        return result;
    }
}
