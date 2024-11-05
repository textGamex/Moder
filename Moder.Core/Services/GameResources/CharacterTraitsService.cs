using System.Collections.Frozen;
using System.Runtime.InteropServices;
using MethodTimer;
using Moder.Core.Extensions;
using Moder.Core.Helper;
using Moder.Core.Models.Character;
using Moder.Core.Models.Modifiers;
using Moder.Core.Services.GameResources.Base;
using NLog.Fluent;
using ParadoxPower.Process;

namespace Moder.Core.Services.GameResources;

public sealed class CharacterTraitsService
    : CommonResourcesService<CharacterTraitsService, FrozenDictionary<string, Trait>>
{
    private Dictionary<string, FrozenDictionary<string, Trait>>.ValueCollection Traits => Resources.Values;

    [Time("加载人物特质")]
    public CharacterTraitsService()
        : base(Path.Combine(Keywords.Common, "unit_leader"), WatcherFilter.Text) { }

    public IEnumerable<Trait> GetAllTraits() => Traits.SelectMany(trait => trait.Values);

    protected override FrozenDictionary<string, Trait>? ParseFileToContent(Node rootNode)
    {
        // Character Traits 和 技能等级修正 在同一个文件夹中, 这里我们只处理 Character Traits 文件
        var traitsNodes = Array.FindAll(
            rootNode.AllArray,
            child =>
                child.IsNodeChild && StringComparer.OrdinalIgnoreCase.Equals(child.node.Key, "leader_traits")
        );
        if (traitsNodes.Length == 0)
        {
            return null;
        }

        // 在 1.14 版本中, 人物特质文件中大约有 145 个特质
        var dictionary = new Dictionary<string, Trait>(163, StringComparer.OrdinalIgnoreCase);
        foreach (var traitsChild in traitsNodes)
        {
            foreach (var traits in ParseTraitsNode(traitsChild.node))
            {
                dictionary[traits.Name] = traits;
            }
        }

        return dictionary.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="traitsNode">文件中的 leader_traits 节点</param>
    /// <returns></returns>
    private ReadOnlySpan<Trait> ParseTraitsNode(Node traitsNode)
    {
        var traits = new List<Trait>(traitsNode.AllArray.Length);

        foreach (var child in traitsNode.AllArray)
        {
            if (!child.IsNodeChild)
            {
                continue;
            }

            var traitNode = child.node;
            var traitName = traitNode.Key;

            var modifiers = new List<ModifierCollection>(4);
            var traitType = TraitType.None;
            foreach (var traitAttribute in traitNode.AllArray)
            {
                var key = traitAttribute.GetKeyOrNull();
                // type 可以为 Leaf 或 Node
                if (StringComparer.OrdinalIgnoreCase.Equals(key, "type"))
                {
                    traitType = GetTraitType(traitAttribute);
                }
                else if (
                    traitAttribute.IsNodeChild
                    && Array.Exists(ModifierNodeKeys, s => StringComparer.OrdinalIgnoreCase.Equals(s, key))
                )
                {
                    modifiers.Add(ModifierHelper.ParseModifier(traitAttribute.node));
                }
            }
            traits.Add(new Trait(traitName, traitType, modifiers));
        }

        return CollectionsMarshal.AsSpan(traits);
    }

    // TODO: 等 .NET 9 发布后, 改用 SearchValues
    private static readonly string[] ModifierNodeKeys =
    [
        "modifier",
        "non_shared_modifier",
        "corps_commander_modifier",
        "field_marshal_modifier",
        "sub_unit_modifiers"
    ];

    private TraitType GetTraitType(Child traitAttribute)
    {
        var traitType = TraitType.None;
        foreach (var traitTypeString in GetTraitTypes(traitAttribute))
        {
            traitType |= GetTraitType(traitTypeString);
        }

        return traitType;
    }

    private static List<string> GetTraitTypes(Child traitTypeAttribute)
    {
        var list = new List<string>(1);
        if (traitTypeAttribute.IsLeafChild)
        {
            list.Add(traitTypeAttribute.leaf.ValueText);
        }

        if (traitTypeAttribute.IsNodeChild)
        {
            list.AddRange(traitTypeAttribute.node.LeafValues.Select(trait => trait.ValueText));
        }

        return list;
    }

    private TraitType GetTraitType(string? traitType)
    {
        if (traitType is null)
        {
            return TraitType.None;
        }

        if (StringComparer.OrdinalIgnoreCase.Equals(traitType, "land"))
        {
            return TraitType.Land;
        }

        if (StringComparer.OrdinalIgnoreCase.Equals(traitType, "navy"))
        {
            return TraitType.Navy;
        }

        if (StringComparer.OrdinalIgnoreCase.Equals(traitType, "corps_commander"))
        {
            return TraitType.CorpsCommander;
        }

        if (StringComparer.OrdinalIgnoreCase.Equals(traitType, "field_marshal"))
        {
            return TraitType.FieldMarshal;
        }

        if (StringComparer.OrdinalIgnoreCase.Equals(traitType, "operative"))
        {
            return TraitType.Operative;
        }

        if (StringComparer.OrdinalIgnoreCase.Equals(traitType, "all"))
        {
            return TraitType.All;
        }

        Logger.Warn($"Unknown trait type: {traitType}");
        return TraitType.None;
    }
}
