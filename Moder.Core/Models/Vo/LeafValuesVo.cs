using Moder.Core.Extensions;
using Moder.Core.Helper;
using ParadoxPower.Process;

namespace Moder.Core.Models.Vo;

public sealed class LeafValuesVo : ObservableGameValue
{
    public string[] Values { get; }

    public LeafValuesVo(string key, IEnumerable<LeafValue> values, NodeVo parent)
        : base(key, parent)
    {
        Values = values.Select(value => value.Value.ToRawString()).ToArray();
        Type = values.First().Value.ToLocalValueType();
    }

    public Child[] ToLeafValues()
    {
        var leafValues = new Child[Values.Length];
        for (var index = 0; index < Values.Length; index++)
        {
            leafValues[index] = Child.NewLeafValueChild(
                LeafValue.Create(ValueConverterHelper.ToValueType(Type, Values[index]))
            );
        }

        return leafValues;
    }
}
