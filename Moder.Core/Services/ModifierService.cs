using System.Collections.Frozen;
using MethodTimer;
using Moder.Core.Models;
using Moder.Core.Services.GameResources;
using Windows.UI;

namespace Moder.Core.Services;

public sealed class ModifierService
{
    private readonly GameResourcesService _gameResourcesService;

    /// <summary>
    /// 无法在本地化文件中判断类型的修饰符, 在文件中手动设置
    /// </summary>
    private readonly FrozenDictionary<string, ModifierType> _modifierTypes;

    public ModifierService(GameResourcesService gameResourcesService)
    {
        _gameResourcesService = gameResourcesService;
        _modifierTypes = ReadModifierTypes();
    }

    [Time("读取文件中的修饰符类型")]
    private static FrozenDictionary<string, ModifierType> ReadModifierTypes()
    {
        var positiveFilePath = Path.Combine(App.ParserRulesFolder, "PositiveModifier.txt");
        var reversedFilePath = Path.Combine(App.ParserRulesFolder, "ReversedModifier.txt");
        var positives = File.Exists(positiveFilePath) ? File.ReadAllLines(positiveFilePath) : [];
        var reversedModifiers = File.Exists(reversedFilePath) ? File.ReadAllLines(reversedFilePath) : [];

        var modifierTypes = new Dictionary<string, ModifierType>(positives.Length + reversedModifiers.Length);
        foreach (var positive in positives)
        {
            if (!string.IsNullOrWhiteSpace(positive))
            {
                modifierTypes.Add(positive.Trim(), ModifierType.Positive);
            }
        }
        foreach (var modifier in reversedModifiers)
        {
            if (!string.IsNullOrWhiteSpace(modifier))
            {
                modifierTypes.Add(modifier.Trim(), ModifierType.Negative);
            }
        }

        return modifierTypes.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private ModifierType GetModifierType(string modifierName)
    {
        if (_modifierTypes.TryGetValue(modifierName, out var modifierType))
        {
            return modifierType;
        }

        if (_gameResourcesService.Localisation.TryGetModifierTt(modifierName, out var value))
        {
            for (var index = value.Length - 1; index >= 0; index--)
            {
                var c = value[index];
                if (c == '+')
                {
                    return ModifierType.Positive;
                }

                if (c == '-')
                {
                    return ModifierType.Negative;
                }
            }

            return ModifierType.Unknown;
        }

        return ModifierType.Unknown;
    }

    public Color GetModifierColor(Modifier modifier)
    {
        var value = double.Parse(modifier.Value);
        if (value == 0.0)
        {
            return Colors.Yellow;
        }

        var modifierType = GetModifierType(modifier.Name);
        if (modifierType == ModifierType.Unknown)
        {
            return Colors.Black;
        }

        if (value > 0.0)
        {
            if (modifierType == ModifierType.Positive)
            {
                return Colors.Green;
            }

            if (modifierType == ModifierType.Negative)
            {
                return Colors.Red;
            }

            return Colors.Black;
        }

        if (value < 0.0)
        {
            if (modifierType == ModifierType.Positive)
            {
                return Colors.Red;
            }

            if (modifierType == ModifierType.Negative)
            {
                return Colors.Green;
            }

            return Colors.Black;
        }

        return Colors.Black;
    }
}
