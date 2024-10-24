using System.Collections.Frozen;
using System.Runtime.InteropServices;
using Moder.Core.Extensions;
using Moder.Core.Models.Character;
using Moder.Core.Services.GameResources.Base;
using NLog.Fluent;
using ParadoxPower.Process;

namespace Moder.Core.Services.GameResources;

public sealed class CharacterTraitsService
    : CommonResourcesService<CharacterTraitsService, FrozenDictionary<string, Trait>>
{
    public CharacterTraitsService()
        : base(Path.Combine(Keywords.Common, "unit_leader"), WatcherFilter.Text) { }

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

        var dictionary = new Dictionary<string, Trait>(128, StringComparer.OrdinalIgnoreCase);
        foreach (var traitsChild in traitsNodes)
        {
            foreach (var traits in ParseTraitsNode(traitsChild.node))
            {
                dictionary.Add(traits.Name, traits);
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

            foreach (var traitAttribute in traitNode.AllArray)
            {
                // type 可以为 Leaf 或 Node
                if (StringComparer.OrdinalIgnoreCase.Equals(traitAttribute.GetKeyOrNull(), "type"))
                {
                    var traitType = TraitType.None;
                    foreach (var traitTypeString in GetTraitTypes(traitAttribute))
                    {
                        traitType |= GetTraitType(traitTypeString);
                    }
                    traits.Add(new Trait(traitName, traitType));
                    break;
                }
            }
        }

        return CollectionsMarshal.AsSpan(traits);
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
            foreach (var trait in traitTypeAttribute.node.LeafValues)
            {
                list.Add(trait.ValueText);
            }
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
