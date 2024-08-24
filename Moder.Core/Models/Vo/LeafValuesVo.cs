using Moder.Core.Extensions;
using ParadoxPower.Process;

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
}
