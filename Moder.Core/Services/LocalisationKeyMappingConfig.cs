namespace Moder.Core.Services;

public sealed class LocalisationKeyMappingConfig
{
    public string LocalisationKey { get; }

    /// <summary>
    /// 当本地化文本中存在占位符时, 该占位符对应的Key
    /// <para/>
    /// 例如: 本地化文本为 trait_bonus_attack:"进攻：$VAL|+=0$" 占位符为 VAL
    /// </summary>
    /// <remarks>
    /// 大小写敏感
    /// </remarks>
    public string ValuePlaceholderKey { get; }

    public LocalisationKeyMappingConfig(string localisationKey, string valuePlaceholderKey = "")
    {
        LocalisationKey = localisationKey;
        ValuePlaceholderKey = valuePlaceholderKey;
    }

    public bool ExistsValuePlaceholder => ValuePlaceholderKey != string.Empty;
}