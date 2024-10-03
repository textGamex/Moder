using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Extensions;
using Moder.Core.Helper;
using ParadoxPower.Process;

namespace Moder.Core.Models.Vo;

public sealed partial class LeafValuesVo : ObservableGameValue
{
    public ObservableCollection<string> Values { get; }

    public LeafValuesVo(string key, IEnumerable<LeafValue> values, NodeVo parent)
        : base(key, parent)
    {
        Values = new ObservableCollection<string>(values.Select(value => value.Value.ToRawString()));
        Type = values.First().Value.ToLocalValueType();
    }

    public LeafValuesVo(string key, IEnumerable<string> values, GameValueType type, NodeVo parent) : base(key, parent)
    {
        Values = new ObservableCollection<string>(values);
        Type = type;
    }

    public Child[] ToLeafValues()
    {
        var leafValues = new Child[Values.Count];
        for (var index = 0; index < Values.Count; index++)
        {
            leafValues[index] = Child.NewLeafValueChild(
                LeafValue.Create(ValueConverterHelper.ToValueType(Type, Values[index]))
            );
        }

        return leafValues;
    }

    [RelayCommand]
    private void RemoveValue(string? value)
    {
        if (value is null)
        {
            return;
        }

        Values.Remove(value);
    }

    [RelayCommand]
    private void AddValue(TextBox textBox)
    {
        var value = textBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        Values.Add(value);

        textBox.Text = string.Empty;
    }
}
