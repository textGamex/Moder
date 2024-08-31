using CommunityToolkit.Mvvm.ComponentModel;
using Moder.Core.Extensions;
using Moder.Core.Helper;
using ParadoxPower.Parser;

namespace Moder.Core.Models.Vo;

public partial class LeafVo : ObservableGameValue
{
	public string[] StateCategory { get; } = ["town", "city", "village"];
    [ObservableProperty]
    private string _value;

    public LeafVo(string key, Types.Value value) : base(key)
    {
        _value = value.ToRawString();
		Type = value.ToLocalValueType();
	}

    public Types.Value ToRawValue()
    {
	    return ValueConverterHelper.ToValueType(Type, Value);
    }
}
