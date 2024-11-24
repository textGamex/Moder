using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using MethodTimer;
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
    private readonly LocalisationKeyMappingService _localisationKeyMapping;

    [Time("加载本地化文件")]
    public LocalisationService(LocalisationKeyMappingService localisationKeyMapping)
        : base(
            Path.Combine(
                "localisation",
                App.Current.Services.GetRequiredService<GlobalSettingService>()
                    .GameLanguage.ToGameLocalizationLanguage()
            ),
            WatcherFilter.LocalizationFiles,
            PathType.Folder
        )
    {
        _localisationKeyMapping = localisationKeyMapping;
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
    /// 查找本地化字符串, 先尝试在 <see cref="LocalisationKeyMappingService"/> 中查找 Key 是否有替换的 Key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValueInAll(string key, [NotNullWhen(true)] out string? value)
    {
        if (_localisationKeyMapping.TryGetValue(key, out var config))
        {
            value = config.LocalisationKey;
            return true;
        }

        return TryGetValue(key, out value);
    }

    public string GetModifier(string modifier)
    {
        if (TryGetValueInAll(modifier, out var value))
        {
            return value;
        }

        if (TryGetValue($"MODIFIER_{modifier}", out value))
        {
            return value;
        }

        if (TryGetValue($"MODIFIER_NAVAL_{modifier}", out value))
        {
            return value;
        }

        if (TryGetValue($"MODIFIER_UNIT_LEADER_{modifier}", out value))
        {
            return value;
        }

        if (TryGetValue($"MODIFIER_ARMY_LEADER_{modifier}", out value))
        {
            return value;
        }

        return modifier;
    }

    public bool TryGetModifierTt(string modifier, [NotNullWhen(true)] out string? result)
    {
        return TryGetValue($"{modifier}_tt", out result);
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
            Logger.LogParseError(localisation.GetError());
            return null;
        }

        var result = localisation.GetResult();
        return result;
    }
}
