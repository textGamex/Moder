using CommunityToolkit.Mvvm.ComponentModel;
using ParadoxPower.Parser;

namespace Moder.Core.Models.Vo;

public partial class IntLeafVo(string key, Types.Value value, NodeVo? parent) : LeafVo(key, value, parent)
{
    [ObservableProperty]
    private int _numberValue = int.TryParse(value.ToRawString(), out var amount) ? amount : 0;

    partial void OnNumberValueChanged(int value)
    {
        Value = value.ToString();
    }
}
