using Moder.Core.Extensions;
using ParadoxPower.Process;

namespace Moder.Core.Models.Vo;

public sealed class LeafValuesVo : ObservableGameValue
{
	public string Key { get; }
	public string[] Values { get; }

	public LeafValuesVo(string key, IEnumerable<LeafValue> values)
	{
		Key = key;
		Values = values.Select(value => value.Value.ToRawString()).ToArray();
		Type = values.First().Value.ToLocalValueType();
	}
}