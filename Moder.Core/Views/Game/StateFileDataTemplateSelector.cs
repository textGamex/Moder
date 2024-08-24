using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Models.Vo;

namespace Moder.Core.Views.Game;

public class StateFileDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? Node { get; set; }
    public DataTemplate? Leaf { get; set; }
    public DataTemplate? LeafValues { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            NodeVo => Node!,
            LeafVo => Leaf!,
            LeafValuesVo => LeafValues!,
            _ => throw new ArgumentException("未知对象", nameof(item))
        };
    }
}
