using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using MethodTimer;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Extensions;
using Moder.Core.Services.Config;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.CSharp;
using ParadoxPower.Localisation;

namespace Moder.Core.Services.GameResources.Localization;

public sealed class LocalizationService
    : ResourcesService<LocalizationService, FrozenDictionary<string, string>, YAMLLocalisationParser.LocFile>
{
    private Dictionary<string, FrozenDictionary<string, string>>.ValueCollection Localisations =>
        Resources.Values;
    private readonly LocalizationKeyMappingService _localizationKeyMapping;

    [Time("加载本地化文件")]
    public LocalizationService(LocalizationKeyMappingService localizationKeyMapping)
        : base(
            Path.Combine(
                "localisation",
                App.Services.GetRequiredService<AppSettingService>().GameLanguage.ToGameLocalizationLanguage()
            ),
            WatcherFilter.LocalizationFiles,
            PathType.Folder
        )
    {
        _localizationKeyMapping = localizationKeyMapping;
    }

    /// <summary>
    /// 如果本地化文本不存在, 则返回<c>key</c>
    /// </summary>
    /// <returns></returns>
    public string GetValue(string key)
    {
        return TryGetValue(key, out var value) ? value : key;
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        foreach (var localisation in Localisations)
        {
            if (localisation.TryGetValue(key, out var result))
            {
                value = result;
                return true;
            }
        }

        value = null;
        return false;
    }

    public string GetValueInAll(string key)
    {
        if (TryGetValueInAll(key, out var value))
        {
            return value;
        }

        return key;
    }

    /// <summary>
    /// 查找本地化字符串, 先尝试在 <see cref="LocalizationKeyMappingService"/> 中查找 Key 是否有替换的 Key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValueInAll(string key, [NotNullWhen(true)] out string? value)
    {
        if (_localizationKeyMapping.TryGetValue(key, out var config))
        {
            key = config.LocalisationKey;
        }

        return TryGetValue(key, out value);
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

        return localisations.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
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
            Log.LogParseError(localisation.GetError());
            return null;
        }

        var result = localisation.GetResult();
        return result;
    }
}
