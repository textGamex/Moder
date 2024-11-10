using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Helper;
using Moder.Core.Services.GameResources;
using ParadoxPower.Parser;
using ParadoxPower.Process;
using ParadoxPower.Utilities;

namespace Moder.Core.Models.Vo;

public partial class LeafVo : ObservableGameValue
{
    public virtual string Value
    {
        get => LeafValue;
        set => SetProperty(ref LeafValue, value);
    }
    protected string LeafValue;

    protected static readonly LocalisationService LocalisationService =
        App.Current.Services.GetRequiredService<LocalisationService>();

    public LeafVo(string key, string value, GameValueType type, NodeVo? parent)
        : base(key, parent)
    {
        LeafValue = value;
        Type = type;
    }

    public Types.Value ToRawValue()
    {
        return ValueConverterHelper.ToValueType(Type, Value);
    }

    public override Child[] ToRawChildren()
    {
        return [Child.NewLeafChild(new Leaf(Key, ToRawValue(), Position.Range.Zero, Types.Operator.Equals))];
    }
}
