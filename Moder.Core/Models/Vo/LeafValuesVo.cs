using Microsoft.FSharp.Core;
using Moder.Core.Extensions;
using Moder.Core.Helper;
using ParadoxPower.Process;
using ParadoxPower.Utilities;

namespace Moder.Core.Models.Vo;

public sealed class LeafValuesVo : ObservableGameValue
{
    public string[] Values { get; }

    public LeafValuesVo(string key, IEnumerable<LeafValue> values)
        : base(key)
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
