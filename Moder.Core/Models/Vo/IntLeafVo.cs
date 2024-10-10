using CommunityToolkit.Mvvm.ComponentModel;

namespace Moder.Core.Models.Vo;

public partial class IntLeafVo(string key, string value, GameValueType type, NodeVo parent) : LeafVo(key, value, type, parent)
{
    [ObservableProperty]
    private int _numberValue = int.TryParse(value, out var amount) ? amount : 0;

    partial void OnNumberValueChanged(int value)
    {
        Value = value.ToString();
    }
}
