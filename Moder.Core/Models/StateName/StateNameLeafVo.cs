using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services;
using ParadoxPower.Parser;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public sealed partial class StateNameLeafVo(string key, Types.Value value, NodeVo? parent) : LeafVo(key, value, parent)
{
    public override string Value
    {
        get => LeafValue;
        set
        {
            SetProperty(ref LeafValue, value);
            OnPropertyChanged(nameof(LocalisedName));
        }
    }

    public string LocalisedName => LocalisationService.GetValue(Value);
}
