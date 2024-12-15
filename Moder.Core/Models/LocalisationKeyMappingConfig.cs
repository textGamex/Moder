namespace Moder.Core.Models;

public sealed class LocalisationKeyMappingConfig(string localisationKey, string valuePlaceholderKey = "")
{
    public string LocalisationKey { get; } = localisationKey;

    /// <summary>
    /// 当本地化文本中存在占位符时, 该占位符对应的Key
    /// <para/>
    /// 例如: 本地化文本为 trait_bonus_attack:"进攻：$VAL|+=0$" 占位符为 VAL
    /// </summary>
    /// <remarks>
    /// 大小写敏感
    /// </remarks>
    public string ValuePlaceholderKey { get; } = valuePlaceholderKey;
    // TODO: 移除占位符, 直接尝试解析并无视占位符, 以此简化代码.
    public bool ExistsValuePlaceholder => ValuePlaceholderKey != string.Empty;
}