using System.Collections.Frozen;
using MethodTimer;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Moder.Core.Models;
using Moder.Core.Models.Modifiers;
using Moder.Core.Services.GameResources;
using NLog;
using Windows.UI;

namespace Moder.Core.Services;

public sealed class ModifierService
{
    private readonly GameResourcesService _gameResourcesService;
    private readonly TerrainService _terrainService;

    /// <summary>
    /// 无法在本地化文件中判断类型的修饰符, 在文件中手动设置
    /// </summary>
    private readonly FrozenDictionary<string, ModifierEffectType> _modifierTypes;

    private static readonly string[] UnitTerrain = ["fort", "river"];
    private static readonly Color Yellow = Color.FromArgb(255, 255, 189, 0);
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public ModifierService(GameResourcesService gameResourcesService, TerrainService terrainService)
    {
        _gameResourcesService = gameResourcesService;
        _terrainService = terrainService;
        _modifierTypes = ReadModifierTypes();
    }

    [Time("读取文件中的修饰符类型")]
    private static FrozenDictionary<string, ModifierEffectType> ReadModifierTypes()
    {
        var positiveFilePath = Path.Combine(App.ParserRulesFolder, "PositiveModifier.txt");
        var reversedFilePath = Path.Combine(App.ParserRulesFolder, "ReversedModifier.txt");
        var positives = File.Exists(positiveFilePath) ? File.ReadAllLines(positiveFilePath) : [];
        var reversedModifiers = File.Exists(reversedFilePath) ? File.ReadAllLines(reversedFilePath) : [];

        var modifierTypes = new Dictionary<string, ModifierEffectType>(
            positives.Length + reversedModifiers.Length
        );
        foreach (var positive in positives)
        {
            if (!string.IsNullOrWhiteSpace(positive))
            {
                modifierTypes.Add(positive.Trim(), ModifierEffectType.Positive);
            }
        }
        foreach (var modifier in reversedModifiers)
        {
            if (!string.IsNullOrWhiteSpace(modifier))
            {
                modifierTypes.Add(modifier.Trim(), ModifierEffectType.Negative);
            }
        }

        return modifierTypes.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private Color GetModifierColor(LeafModifier leafModifier, string modifierTypeDescription)
    {
        var value = double.Parse(leafModifier.Value);
        if (value == 0.0)
        {
            return Yellow;
        }

        var modifierType = GetModifierType(leafModifier.Key, modifierTypeDescription);
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

            return Colors.Black;
        }

        if (value < 0.0)
        {
            if (modifierType == ModifierEffectType.Positive)
            {
                return Colors.Red;
            }

            if (modifierType == ModifierEffectType.Negative)
            {
                return Colors.Green;
            }

            return Colors.Black;
        }

        return Colors.Black;
    }

    private ModifierEffectType GetModifierType(string modifierName, string modifierTypeDescription)
    {
        if (_modifierTypes.TryGetValue(modifierName, out var modifierType))
        {
            return modifierType;
        }

        for (var index = modifierTypeDescription.Length - 1; index >= 0; index--)
        {
            var c = modifierTypeDescription[index];
            if (c == '+')
            {
                return ModifierEffectType.Positive;
            }

            if (c == '-')
            {
                return ModifierEffectType.Negative;
            }
        }

        return ModifierEffectType.Unknown;
    }

    public IReadOnlyCollection<Inline> GetModifierInlines(IEnumerable<IModifier> modifiers)
    {
        var inlines = new List<Inline>(8);

        foreach (var modifier in modifiers)
        {
            List<Inline> addedInlines;
            if (modifier.Type == ModifierType.Leaf)
            {
                var leafModifier = (LeafModifier)modifier;
                addedInlines = GetModifierInlinesForLeaf(leafModifier);
            }
            else if (modifier.Type == ModifierType.Node)
            {
                var nodeModifier = (NodeModifier)modifier;
                addedInlines = GetModifierInlinesForNode(nodeModifier);
            }
            else
            {
                continue;
            }

            inlines.AddRange(addedInlines);
            inlines.Add(new LineBreak());
        }

        if (inlines.Count != 0 && inlines[^1] is LineBreak)
        {
            inlines.RemoveAt(inlines.Count - 1);
        }
        return inlines;
    }

    private List<Inline> GetModifierInlinesForLeaf(LeafModifier modifier)
    {
        var inlines = new List<Inline>(4);
        inlines.Add(new Run { Text = $"{_gameResourcesService.Localisation.GetModifier(modifier.Key)}: " });

        if (modifier.ValueType is GameValueType.Int or GameValueType.Float)
        {
            var modifierTypeDescription = _gameResourcesService.Localisation.TryGetModifierTt(
                modifier.Key,
                out var result
            )
                ? result
                : string.Empty;
            var value = GetModifierDisplayValue(modifier, modifierTypeDescription);
            inlines.Add(
                new Run
                {
                    Text = value,
                    Foreground = new SolidColorBrush(GetModifierColor(modifier, modifierTypeDescription))
                }
            );
        }
        else
        {
            inlines.Add(new Run { Text = modifier.Value });
        }

        return inlines;
    }

    private List<Inline> GetModifierInlinesForNode(NodeModifier nodeModifier)
    {
        if (
            _terrainService.Contains(nodeModifier.Key)
            || Array.Exists(UnitTerrain, element => element == nodeModifier.Key)
        )
        {
            return GetTerrainModifierInlines(nodeModifier);
        }

        Log.Warn("未知的节点修饰符: {Name}", nodeModifier.Key);
        var inlines = new List<Inline>(nodeModifier.Modifiers.Count * 3);
        foreach (var leafModifier in nodeModifier.Modifiers)
        {
            inlines.AddRange(GetModifierInlinesForLeaf(leafModifier));
        }
        return inlines;
    }

    /// <summary>
    /// 获取地形修饰符的描述
    /// </summary>
    /// <param name="nodeModifier"></param>
    /// <returns></returns>
    private List<Inline> GetTerrainModifierInlines(NodeModifier nodeModifier)
    {
        var inlines = new List<Inline>(4);
        var terrainName = _gameResourcesService.Localisation.GetValue(nodeModifier.Key);
        inlines.Add(new Run { Text = $"{terrainName}: " });
        inlines.Add(new LineBreak());

        for (var index = 0; index < nodeModifier.Modifiers.Count; index++)
        {
            var modifier = nodeModifier.Modifiers[index];
            var modifierDescription = _gameResourcesService.Localisation.GetValue(
                $"STAT_ADJUSTER_{modifier.Key}_DIFF"
            );
            var modifierName = _gameResourcesService.Localisation.GetValue($"STAT_ADJUSTER_{modifier.Key}");
            var color = GetModifierColor(modifier, modifierDescription);
            inlines.Add(
                new Run
                {
                    Text = $"  {modifierName}{GetModifierDisplayValue(modifier, modifierDescription)}",
                    Foreground = new SolidColorBrush(color)
                }
            );

            if (index != nodeModifier.Modifiers.Count - 1)
            {
                inlines.Add(new LineBreak());
            }
        }

        return inlines;
    }

    /// <summary>
    /// 获取 Modifier 数值的显示值
    /// </summary>
    /// <param name="leafModifier"></param>
    /// <param name="modifierDescription"></param>
    /// <returns></returns>
    private static string GetModifierDisplayValue(LeafModifier leafModifier, string modifierDescription)
    {
        if (leafModifier.ValueType is GameValueType.Int or GameValueType.Float)
        {
            var value = double.Parse(leafModifier.Value);
            var sign = string.Empty;
            var displayPlaces = '1';
            if (!leafModifier.Value.StartsWith('-'))
            {
                sign = "+";
            }

            for (var i = modifierDescription.Length - 1; i >= 0; i--)
            {
                var c = modifierDescription[i];
                if (char.IsDigit(c))
                {
                    displayPlaces = c;
                    break;
                }
            }

            return $"{sign}{value.ToString($"P{displayPlaces}")}";
        }

        return leafModifier.Value;
    }
}
