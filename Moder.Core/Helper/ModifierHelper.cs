using Moder.Core.Models.Modifiers;
using ParadoxPower.Process;

namespace Moder.Core.Helper;

public static class ModifierHelper
{
    public static ModifierCollection ParseModifier(Node modifierNode)
    {
        var list = new List<IModifier>(modifierNode.AllArray.Length);
        foreach (var child in modifierNode.AllArray)
        {
            if (child.IsLeafChild)
            {
                var modifier = LeafModifier.FromLeaf(child.leaf);
                list.Add(modifier);
            }
            else if (child.IsNodeChild)
            {
                var node = child.node;
                var modifier = new NodeModifier(node.Key, node.Leaves.Select(LeafModifier.FromLeaf));
                list.Add(modifier);
            }
        }

        return new ModifierCollection(modifierNode.Key, list);
    }
}
