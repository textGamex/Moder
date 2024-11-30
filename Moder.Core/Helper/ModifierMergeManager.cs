using System.Globalization;
using System.Runtime.InteropServices;
using Moder.Core.Models;
using Moder.Core.Models.Modifiers;
using NLog;

namespace Moder.Core.Helper;

/// <summary>
/// 修饰符合并管理器
/// </summary>
public sealed class ModifierMergeManager
{
    private readonly Dictionary<string, decimal> _leafModifiers = [];
    private readonly Dictionary<string, Dictionary<string, decimal>> _nodeModifiers = [];
    private readonly List<LeafModifier> _customEffectTooltipLocalizationKeys = [];

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public void Add(IModifier modifier)
    {
        if (modifier.Type == ModifierType.Leaf)
        {
            AddLeafModifier((LeafModifier)modifier);
        }
        else if (modifier.Type == ModifierType.Node)
        {
            AddNodeModifier((NodeModifier)modifier);
        }
        else
        {
            Log.Warn("Unknown modifier type: {Type}", modifier.Type);
        }
    }

    public void AddRange(IEnumerable<IModifier> modifiers)
    {
        foreach (var modifier in modifiers)
        {
            Add(modifier);
        }
    }

    public void Remove(IModifier modifier)
    {
        if (modifier.Type == ModifierType.Leaf)
        {
            RemoveLeafModifier((LeafModifier)modifier);
        }
        else if (modifier.Type == ModifierType.Node)
        {
            RemoveNodeModifier((NodeModifier)modifier);
        }
        else
        {
            Log.Warn("Unknown modifier type: {Type}", modifier.Type);
        }
    }

    public void RemoveAll(IEnumerable<IModifier> modifiers)
    {
        foreach (var modifier in modifiers)
        {
            Remove(modifier);
        }
    }

    private void RemoveNodeModifier(NodeModifier removedModifier)
    {
        if (_nodeModifiers.TryGetValue(removedModifier.Key, out var modifierMap))
        {
            foreach (var removedLeafModifier in removedModifier.Modifiers)
            {
                RemoveLeafModifierInDictionary(modifierMap, removedLeafModifier);
            }

            if (modifierMap.Count == 0)
            {
                _nodeModifiers.Remove(removedModifier.Key);
            }
        }
    }

    private void RemoveLeafModifier(LeafModifier modifier)
    {
        if (StringComparer.OrdinalIgnoreCase.Equals(modifier.Key, LeafModifier.CustomEffectTooltipKey))
        {
            _customEffectTooltipLocalizationKeys.Remove(modifier);
            return;
        }

        RemoveLeafModifierInDictionary(_leafModifiers, modifier);
    }

    private static void RemoveLeafModifierInDictionary(
        Dictionary<string, decimal> dictionary,
        LeafModifier modifier
    )
    {
        var value = decimal.TryParse(modifier.Value, out var result) ? result : 0;
        if (dictionary.TryGetValue(modifier.Key, out var rawValue))
        {
            var newValue = rawValue - value;
            if (newValue == decimal.Zero)
            {
                dictionary.Remove(modifier.Key);
            }
            else
            {
                dictionary[modifier.Key] = newValue;
            }
            return;
        }

        dictionary[modifier.Key] = -value;
    }

    public IEnumerable<IModifier> GetMergedModifiers()
    {
        var leafModifiers = _leafModifiers.Select(
            IModifier (pair) =>
                new LeafModifier(
                    pair.Key,
                    pair.Value.ToString(CultureInfo.InvariantCulture),
                    GameValueType.Float
                )
        );
        var nodeModifiers = _nodeModifiers.Select(
            IModifier (pair) =>
                new NodeModifier(
                    pair.Key,
                    pair.Value.Select(y => new LeafModifier(
                        y.Key,
                        y.Value.ToString(CultureInfo.InvariantCulture),
                        GameValueType.Float
                    ))
                )
        );

        return _customEffectTooltipLocalizationKeys.Concat(leafModifiers).Concat(nodeModifiers);
    }

    private void AddLeafModifier(LeafModifier modifier)
    {
        if (StringComparer.OrdinalIgnoreCase.Equals(LeafModifier.CustomEffectTooltipKey, modifier.Key))
        {
            _customEffectTooltipLocalizationKeys.Add(modifier);
            return;
        }

        AddLeafModifierToDictionary(_leafModifiers, modifier);
    }

    private void AddNodeModifier(NodeModifier modifier)
    {
        if (_nodeModifiers.TryGetValue(modifier.Key, out var modifierMap))
        {
            foreach (var leafModifier in modifier.Modifiers)
            {
                AddLeafModifierToDictionary(modifierMap, leafModifier);
            }
        }
        else
        {
            var newModifierMap = new Dictionary<string, decimal>(modifier.Modifiers.Count);
            foreach (var leafModifier in modifier.Modifiers)
            {
                var value = decimal.TryParse(leafModifier.Value, out var result) ? result : 0;
                newModifierMap[leafModifier.Key] = value;
            }
            _nodeModifiers.Add(modifier.Key, newModifierMap);
        }
    }

    private static void AddLeafModifierToDictionary(
        Dictionary<string, decimal> dictionary,
        LeafModifier modifier
    )
    {
        var value = decimal.TryParse(modifier.Value, out var result) ? result : 0;
        if (value == decimal.Zero)
        {
            return;
        }

        ref var refValue = ref CollectionsMarshal.GetValueRefOrAddDefault(
            dictionary,
            modifier.Key,
            out var exists
        );
        if (exists)
        {
            var newValue = refValue + value;
            if (newValue == decimal.Zero)
            {
                dictionary.Remove(modifier.Key);
            }
            else
            {
                refValue = newValue;
            }
        }
        else
        {
            refValue = value;
        }
    }
}
