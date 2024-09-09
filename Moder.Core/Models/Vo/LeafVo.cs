using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Extensions;
using Moder.Core.Helper;
using Moder.Core.Services;
using ParadoxPower.Parser;

namespace Moder.Core.Models.Vo;

public partial class LeafVo : ObservableGameValue
{
    public static string[] StateCategory { get; } =
        App.Current.Services.GetRequiredService<GameResourcesService>().StateCategory;

    public NodeVo Parent { get; }

    [ObservableProperty]
    private string _value;

    public LeafVo(string key, Types.Value value, NodeVo parent)
        : base(key)
    {
        _value = value.ToRawString();
        Type = value.ToLocalValueType();
        Parent = parent;
    }

    public Types.Value ToRawValue()
    {
        return ValueConverterHelper.ToValueType(Type, Value);
    }

    [RelayCommand]
    private void RemoveSelfInParent()
    {
        Parent.Remove(this);
    }
}
