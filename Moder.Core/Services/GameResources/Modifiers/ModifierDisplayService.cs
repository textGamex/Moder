using Avalonia.Controls.Documents;
using Avalonia.Media;
using Moder.Core.Models.Game;
using Moder.Core.Models.Game.Character;
using Moder.Core.Models.Game.Modifiers;
using Moder.Core.Services.GameResources.Localization;
using Moder.Language.Strings;
using NLog;

namespace Moder.Core.Services.GameResources.Modifiers;

public sealed class ModifierDisplayService
{
    private readonly LocalizationFormatService _localisationFormatService;
    private readonly LocalizationService _localizationService;
    private readonly ModifierService _modifierService;
    private readonly TerrainService _terrainService;
    private readonly LocalizationKeyMappingService _localisationKeyMappingService;
    private readonly CharacterSkillService _characterSkillService;

    private static readonly string[] UnitTerrain = ["fort", "river"];
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public ModifierDisplayService(
        LocalizationFormatService localisationFormatService,
        LocalizationService localizationService,
        ModifierService modifierService,
        LocalizationKeyMappingService localisationKeyMappingService,
        TerrainService terrainService,
        CharacterSkillService characterSkillService
    )
    {
        _localisationFormatService = localisationFormatService;
        _localizationService = localizationService;
        _modifierService = modifierService;
        _localisationKeyMappingService = localisationKeyMappingService;
        _terrainService = terrainService;
        _characterSkillService = characterSkillService;
    }

    public IEnumerable<Inline> GetSkillModifierDescription(
        SkillType skillType,
        SkillCharacterType skillCharacterType,
        ushort level
    )
    {
        var skillModifier = _characterSkillService.Skills
            .FirstOrDefault(skill => skill.SkillType == skillType)
            ?.GetModifierDescription(skillCharacterType, level);

        if (skillModifier is null || skillModifier.Modifiers.Count == 0)
        {
            return [new Run { Text = Resource.CharacterEditor_None }];
        }

        return GetModifierInlines(skillModifier.Modifiers);
    }

    public IReadOnlyCollection<Inline> GetModifierInlines(IEnumerable<IModifier> modifiers)
    {
        var inlines = new List<Inline>(8);

        foreach (var modifier in modifiers)
        {
            IEnumerable<Inline> addedInlines;
            if (modifier.Type == ModifierType.Leaf)
            {
                var leafModifier = (LeafModifier)modifier;
                if (IsCustomToolTip(leafModifier.Key))
                {
                    var name = _localizationService.GetValue(leafModifier.Value);
                    addedInlines = _localisationFormatService
                        .GetColorText(name)
                        .Select(colorTextInfo => new Run(colorTextInfo.DisplayText)
                        {
                            Foreground = colorTextInfo.Brush
                        });
                }
                else
                {
                    addedInlines = GetModifierInlinesForLeaf(leafModifier);
                }
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

        RemoveLastLineBreak(inlines);
        return inlines;
    }

    private static bool IsCustomToolTip(string modifierKey)
    {
        return StringComparer.OrdinalIgnoreCase.Equals(modifierKey, LeafModifier.CustomEffectTooltipKey)
            || StringComparer.OrdinalIgnoreCase.Equals(modifierKey, LeafModifier.CustomModifierTooltipKey);
    }

    private List<Inline> GetModifierInlinesForLeaf(LeafModifier modifier)
    {
        // if (_localisationKeyMappingService.TryGetValue(modifier.Key, out var config))
        // {
        //     return GetModifierDisplayMessageForMapping(modifier, config);
        // }

        return GetModifierDisplayMessageUniversal(modifier);
    }

    private List<Inline> GetModifierDisplayMessageUniversal(LeafModifier modifier)
    {
        var inlines = new List<Inline>(4);
        GetModifierColorTextFromText(modifier.Key, inlines);

        if (modifier.ValueType is GameValueType.Int or GameValueType.Float)
        {
            var modifierFormat = _modifierService.TryGetLocalizationTt(modifier.Key, out var result)
                ? result
                : string.Empty;
            var value = _modifierService.GetModifierDisplayValue(modifier, modifierFormat);
            inlines.Add(
                new Run
                {
                    Text = value,
                    Foreground = new SolidColorBrush(
                        _modifierService.GetModifierColor(modifier, modifierFormat)
                    )
                }
            );
        }
        else
        {
            inlines.Add(new Run { Text = modifier.Value });
        }
        return inlines;
    }

    private void GetModifierColorTextFromText(string modifierKey, List<Inline> inlines)
    {
        var modifierName = _modifierService.GetLocalizationName(modifierKey);
        foreach (var colorTextInfo in _localisationFormatService.GetColorText(modifierName))
        {
            inlines.Add(new Run(colorTextInfo.DisplayText) { Foreground = colorTextInfo.Brush });
        }
    }

    private List<Inline> GetModifierInlinesForNode(NodeModifier nodeModifier)
    {
        // TODO: 移动到 TerrainService???
        // if (
        //     _terrainService.Contains(nodeModifier.Key)
        //     || Array.Exists(UnitTerrain, element => element == nodeModifier.Key)
        // )
        // {
        //     return GetTerrainModifierInlines(nodeModifier);
        // }

        Log.Warn("未知的节点修饰符: {Name}", nodeModifier.Key);
        var inlines = new List<Inline>(nodeModifier.Modifiers.Count * 3);
        foreach (var leafModifier in nodeModifier.Modifiers)
        {
            inlines.AddRange(GetModifierInlinesForLeaf(leafModifier));
        }
        return inlines;
    }

    private static void RemoveLastLineBreak(List<Inline> inlines)
    {
        if (inlines.Count != 0 && inlines[^1] is LineBreak)
        {
            inlines.RemoveAt(inlines.Count - 1);
        }
    }
}
