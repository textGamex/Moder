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
    public DataTemplate IntLeaf { get; set; } = null!;
    public DataTemplate FloatLeaf { get; set; } = null!;
    public DataTemplate LeafValues { get; set; } = null!;
    public DataTemplate Comment { get; set; } = null!;
    public DataTemplate StateCategoryLeaf { get; set; } = null!;
    public DataTemplate BuildingLeaf { get; set; } = null!;
    public DataTemplate NameLeaf { get; set; } = null!;
    public DataTemplate CountryTagLeaf { get; set; } = null!;
    public DataTemplate ResourcesLeaf { get; set; } = null!;

    protected override DataTemplate SelectTemplateCore(object item)
    {
        AssertTemplatesIsNotNull();

        return item switch
        {
            StateCategoryLeafVo => StateCategoryLeaf,
            StateNameLeafVo => NameLeaf,
            BuildingLeafVo => BuildingLeaf,
            CountryTagLeafVo => CountryTagLeaf,
            ResourcesLeafVo => ResourcesLeaf,
            IntLeafVo => IntLeaf,
            FloatLeafVo => FloatLeaf,

            NodeVo => Node,
            LeafVo => Leaf,
            LeafValuesVo => LeafValues,
            CommentVo => Comment,
            _ => throw new ArgumentException("未知对象", nameof(item))
        };
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
