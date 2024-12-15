using System.Diagnostics.CodeAnalysis;
using Avalonia.Media;
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
    private static readonly Color Yellow = Color.FromRgb(255, 189, 0);

    public Color GetModifierColor(LeafModifier leafModifier, string modifierFormat)
    {
        var value = double.Parse(leafModifier.Value);
        if (value == 0.0)
        {
            return Yellow;
        }

        var modifierType = GetModifierType(leafModifier.Key, modifierFormat);
        if (modifierType == ModifierEffectType.Unknown)
        {
            return Colors.Black;
        }

        if (value > 0.0)
        {
            if (modifierType == ModifierEffectType.Positive)
            {
                return Colors.Green;
            }

            if (modifierType == ModifierEffectType.Negative)
            {
                return Colors.Red;
            }
        }
        else
        {
            if (modifierType == ModifierEffectType.Positive)
            {
                return Colors.Red;
            }

            if (modifierType == ModifierEffectType.Negative)
            {
                return Colors.Green;
            }
        }

        return Colors.Black;
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

    public bool TryGetLocalizationTt(string modifier, [NotNullWhen(true)] out string? result)
    {
        return _localizationService.TryGetValue($"{modifier}_tt", out result);
    }

    private static ModifierEffectType GetModifierType(string modifierName, string modifierFormat)
    {
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
    /// <param name="leafModifier"></param>
    /// <param name="modifierDisplayFormat"></param>
    /// <returns></returns>
    public string GetModifierDisplayValue(LeafModifier leafModifier, string modifierDisplayFormat)
    {
        if (leafModifier.ValueType is GameValueType.Int or GameValueType.Float)
        {
            var value = double.Parse(leafModifier.Value);
            var sign = leafModifier.Value.StartsWith('-') ? string.Empty : "+";

            var displayDigits = GetModifierDisplayDigits(modifierDisplayFormat);
            var isPercentage =
                string.IsNullOrEmpty(modifierDisplayFormat) || modifierDisplayFormat.Contains('%');
            var format = isPercentage ? 'P' : 'F';

            return $"{sign}{value.ToString($"{format}{displayDigits}")}";
        }

        return leafModifier.Value;
    }

    private static char GetModifierDisplayDigits(string modifierDescription)
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