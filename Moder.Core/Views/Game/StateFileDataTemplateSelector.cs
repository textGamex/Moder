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
        AssertTemplatesIsNotNull();

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
        if (leaf.Key.Equals("state_category", StringComparison.OrdinalIgnoreCase))
        {
            return StateCategoryLeaf;
        }
        return Leaf;
    }

    [Conditional("DEBUG")]
    private void AssertTemplatesIsNotNull()
    {
        foreach (
            var propertyInfo in typeof(StateFileDataTemplateSelector)
                .GetProperties()
                .Where(info => info.PropertyType == typeof(DataTemplate))
        )
        {
            var template = propertyInfo.GetValue(this) as DataTemplate;
            Debug.Assert(template is not null, propertyInfo.Name + " is null");
        }
    }
}
