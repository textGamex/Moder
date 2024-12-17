using System.Diagnostics.CodeAnalysis;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Moder.Core.Models.Game;
using Moder.Core.Models.Game.Modifiers;
using Moder.Core.Services.GameResources.Localization;
using NLog;

namespace Moder.Core.Services.GameResources.Modifiers;

public sealed class ModifierService
{
    private readonly LocalizationService _localizationService;

    public ModifierService(LocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private static readonly ImmutableSolidColorBrush Yellow = new(Color.FromRgb(255, 189, 0));

    public IBrush GetModifierBrush(LeafModifier leafModifier, string modifierFormat)
    {
        var value = double.Parse(leafModifier.Value);
        if (value == 0.0)
        {
            return Yellow;
        }

        var modifierType = GetModifierType(leafModifier.Key, modifierFormat);
        if (modifierType == ModifierEffectType.Unknown)
        {
            return Brushes.Black;
        }

        if (value > 0.0)
        {
            if (modifierType == ModifierEffectType.Positive)
            {
                return Brushes.Green;
            }

            if (modifierType == ModifierEffectType.Negative)
            {
                return Brushes.Red;
            }
        }
        else
        {
            if (modifierType == ModifierEffectType.Positive)
            {
                return Brushes.Red;
            }

            if (modifierType == ModifierEffectType.Negative)
            {
                return Brushes.Green;
            }
        }

        return Brushes.Black;
    }

    public string GetLocalizationName(string modifier)
    {
        if (_localizationService.TryGetValueInAll(modifier, out var value))
        {
            return value;
        }

        if (_localizationService.TryGetValue($"MODIFIER_{modifier}", out value))
        {
            return value;
        }

        if (_localizationService.TryGetValue($"MODIFIER_NAVAL_{modifier}", out value))
        {
            return value;
        }

        if (_localizationService.TryGetValue($"MODIFIER_UNIT_LEADER_{modifier}", out value))
        {
            return value;
        }

        if (_localizationService.TryGetValue($"MODIFIER_ARMY_LEADER_{modifier}", out value))
        {
            return value;
        }

        return modifier;
    }

    public bool TryGetLocalizationFormat(string modifier, [NotNullWhen(true)] out string? result)
    {
        if (_localizationService.TryGetValue($"{modifier}_tt", out result))
        {
            return true;
        }

        return _localizationService.TryGetValue(modifier, out result);
    }

    private static ModifierEffectType GetModifierType(string modifierName, string modifierFormat)
    {
        // TODO: 重新支持从数据库中定义修饰符
        // if (_modifierTypes.TryGetValue(modifierName, out var modifierType))
        // {
        //     return modifierType;
        // }

        for (var index = modifierFormat.Length - 1; index >= 0; index--)
        {
            var c = modifierFormat[index];
            switch (c)
            {
                case '+':
                    return ModifierEffectType.Positive;
                case '-':
                    return ModifierEffectType.Negative;
            }
        }

        return ModifierEffectType.Unknown;
    }

    /// <summary>
    /// 获取 Modifier 数值的显示值
    /// </summary>
    /// <param name="leafModifier">包含关键字和对应值的修饰符对象</param>
    /// <param name="modifierDisplayFormat">修饰符对应的格式化设置文本, 为空时使用百分比格式</param>
    /// <returns>应用<c>modifierDisplayFormat</c>格式的<c>LeafModifier.Value</c>的的显示值</returns>
    public string GetDisplayValue(LeafModifier leafModifier, string modifierDisplayFormat)
    {
        if (leafModifier.ValueType is GameValueType.Int or GameValueType.Float)
        {
            var value = double.Parse(leafModifier.Value);
            var sign = leafModifier.Value.StartsWith('-') ? string.Empty : "+";

            var displayDigits = GetDisplayDigits(modifierDisplayFormat);
            var isPercentage =
                string.IsNullOrEmpty(modifierDisplayFormat) || modifierDisplayFormat.Contains('%');
            var format = isPercentage ? 'P' : 'F';

            return $"{sign}{value.ToString($"{format}{displayDigits}")}";
        }

        return leafModifier.Value;
    }

    private static bool IsAllUppercase(string value)
    {
        return value.All(char.IsUpper);
    }

    private static char GetDisplayDigits(string modifierDescription)
    {
        var displayDigits = '1';
        for (var i = modifierDescription.Length - 1; i >= 0; i--)
        {
            var c = modifierDescription[i];
            if (char.IsDigit(c))
            {
                displayDigits = c;
                break;
            }
        }

        return displayDigits;
    }
}
