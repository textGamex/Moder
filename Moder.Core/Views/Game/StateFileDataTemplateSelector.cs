using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Models.Vo;

namespace Moder.Core.Views.Game;

public partial class StateFileDataTemplateSelector : DataTemplateSelector
{
    // 每个 DataTemplate 都需要在 XAML 中声明
    public DataTemplate Node { get; set; } = null!;
    public DataTemplate Leaf { get; set; } = null!;
    public DataTemplate LeafValues { get; set; } = null!;
    public DataTemplate StateCategoryLeaf { get; set; } = null!;

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            NodeVo => Node,
            LeafVo leafVo => GetConcreteLeafTemplate(leafVo),
            LeafValuesVo => LeafValues,
            _ => throw new ArgumentException("未知对象", nameof(item))
        };
    }

    private DataTemplate GetConcreteLeafTemplate(LeafVo leaf)
    {
        Debug.Assert(StateCategoryLeaf is not null);
        Debug.Assert(Leaf is not null);

        if (leaf.Key.Equals("state_category", StringComparison.OrdinalIgnoreCase))
        {
            return StateCategoryLeaf;
        }
        return Leaf;
    }
}
