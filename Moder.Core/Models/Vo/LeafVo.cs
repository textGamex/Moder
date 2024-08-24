using CommunityToolkit.Mvvm.ComponentModel;
using Moder.Core.Extensions;
using ParadoxPower.Parser;

namespace Moder.Core.Models.Vo;

public partial class LeafVo : ObservableGameValue
{
    [ObservableProperty]
    private string _value;

    public LeafVo(string key, Types.Value value) : base(key)
    {
        _value = value.ToRawString();
		Type = value.ToLocalValueType();
	}

    public Types.Value ToRawValue()
    {
        return Type switch
        {
            GameValueType.Bool => Types.Value.NewBool(bool.Parse(Value)),
            GameValueType.Float => Types.Value.NewFloat(decimal.Parse(Value)),
            GameValueType.Int => Types.Value.NewInt(int.Parse(Value)),
            GameValueType.String => Types.Value.NewStringValue(Value),
            GameValueType.StringWithQuotation => Types.Value.NewQStringValue(Value),
            GameValueType.None => throw new ArgumentException(),
            _ => throw new ArgumentException()
        };
    }
}
