using Moder.Core.Extensions;
using Moder.Core.Helper;
using ParadoxPower.Parser;

namespace Moder.Core.Models.Vo;

public partial class LeafVo : ObservableGameValue
{
    public virtual string Value
    {
        get => _value;
        set => SetProperty(ref this._value, value);
    }
    // ReSharper disable once InconsistentNaming
    protected string _value;

    public LeafVo(string key, Types.Value value, NodeVo? parent)
        : base(key, parent)
    {
        _value = value.ToRawString();
        Type = value.ToLocalValueType();
    }

    public Types.Value ToRawValue()
    {
        return ValueConverterHelper.ToValueType(Type, Value);
    }
}
