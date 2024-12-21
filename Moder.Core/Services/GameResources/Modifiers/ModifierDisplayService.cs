﻿using Avalonia.Controls.Documents;
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

    private const string NodeModifierChildrenPrefix = "  ";
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
        var skillModifier = _characterSkillService
            .Skills.FirstOrDefault(skill => skill.SkillType == skillType)
            ?.GetModifierDescription(skillCharacterType, level);

        if (skillModifier is null || skillModifier.Modifiers.Count == 0)
        {
            return [new Run { Text = Resource.CharacterEditor_None }];
        }

        return GetDescription(skillModifier.Modifiers);
    }

    public IReadOnlyCollection<Inline> GetDescription(IEnumerable<IModifier> modifiers)
    {
        var inlines = new List<Inline>(8);

        foreach (var modifier in modifiers)
        {
            IEnumerable<Inline> addedInlines;
            switch (modifier.Type)
            {
                case ModifierType.Leaf:
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
                        addedInlines = GetDescriptionForLeaf(leafModifier);
                    }

                    break;
                }
                case ModifierType.Node:
                {
                    var nodeModifier = (NodeModifier)modifier;
                    addedInlines = GetModifierDescriptionForNode(nodeModifier);
                    break;
                }
                default:
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

    private List<Run> GetDescriptionForLeaf(LeafModifier modifier)
    {
        var modifierKey = _localisationKeyMappingService.TryGetValue(modifier.Key, out var mappingKey)
            ? mappingKey
            : modifier.Key;
        var inlines = new List<Run>(4);
        GetModifierColorTextFromText(modifierKey, inlines);

        if (modifier.ValueType is GameValueType.Int or GameValueType.Float)
        {
            var modifierFormat = _modifierService.TryGetLocalizationFormat(modifierKey, out var result)
                ? result
                : string.Empty;
            inlines.Add(GetRun(modifier, modifierFormat));
        }
        else
        {
            inlines.Add(new Run { Text = modifier.Value });
        }
        return inlines;
    }

    private void GetModifierColorTextFromText(string modifierKey, List<Run> inlines)
    {
        var modifierName = _modifierService.GetLocalizationName(modifierKey);
        foreach (var colorTextInfo in _localisationFormatService.GetColorText(modifierName))
        {
            inlines.Add(new Run(colorTextInfo.DisplayText) { Foreground = colorTextInfo.Brush });
        }
    }

    private List<Inline> GetModifierDescriptionForNode(NodeModifier nodeModifier)
    {
        if (_terrainService.Contains(nodeModifier.Key))
        {
            return GetTerrainModifierDescription(nodeModifier);
        }

        return GetDescriptionForUnknownNode(nodeModifier);
    }

    /// <summary>
    /// 获取地形修饰符的描述
    /// </summary>
    /// <param name="nodeModifier"></param>
    /// <returns></returns>
    private List<Inline> GetTerrainModifierDescription(NodeModifier nodeModifier)
    {
        return GetDescriptionForNode(
            nodeModifier,
            leafModifier =>
            {
                var modifierName = _localizationService.GetValue($"STAT_ADJUSTER_{leafModifier.Key}");
                var modifierFormat = _localizationService.GetValue($"STAT_ADJUSTER_{leafModifier.Key}_DIFF");
                return
                [
                    new Run { Text = $"{NodeModifierChildrenPrefix}{modifierName}" },
                    GetRun(leafModifier, modifierFormat)
                ];
            }
        );
    }

    /// <summary>
    /// 从 <see cref="LeafModifier"/> 中获取<c>LeafModifier.Value</c>的 <see cref="Run"/>, 并使用<c>modifierFormat</c>设置值的格式
    /// </summary>
    /// <param name="modifier"></param>
    /// <param name="modifierFormat"></param>
    /// <returns></returns>
    private Run GetRun(LeafModifier modifier, string modifierFormat)
    {
        return new Run
        {
            Text = _modifierService.GetDisplayValue(modifier, modifierFormat),
            Foreground = _modifierService.GetModifierBrush(modifier, modifierFormat)
        };
    }

    private List<Inline> GetDescriptionForUnknownNode(NodeModifier nodeModifier)
    {
        Log.Warn("未知的节点修饰符: {Name}", nodeModifier.Key);
        return GetDescriptionForNode(
            nodeModifier,
            leafModifier =>
            {
                var runs = GetDescriptionForLeaf(leafModifier);
                foreach (var run in runs)
                {
                    run.Text = $"{NodeModifierChildrenPrefix}{run.Text}";
                }

                return runs;
            }
        );
    }

    private List<Inline> GetDescriptionForNode(
        NodeModifier nodeModifier,
        Func<LeafModifier, IEnumerable<Inline>> func
    )
    {
        var inlines = new List<Inline>(nodeModifier.Modifiers.Count * 3)
        {
            new Run { Text = $"{_localizationService.GetValue(nodeModifier.Key)}:" },
            new LineBreak()
        };

        foreach (var leafModifier in nodeModifier.Modifiers)
        {
            inlines.AddRange(func(leafModifier));
            inlines.Add(new LineBreak());
        }

        RemoveLastLineBreak(inlines);
        return inlines;
    }

    /// <summary>
    /// 移除末尾多余的换行
    /// </summary>
    /// <param name="inlines">一段文本的 <see cref="Inline"/> 集合</param>
    private static void RemoveLastLineBreak(List<Inline> inlines)
    {
        if (inlines.Count != 0 && inlines[^1] is LineBreak)
        {
            inlines.RemoveAt(inlines.Count - 1);
        }
    }
}
