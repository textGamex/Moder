using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ParadoxPower.Parser;

namespace Moder.Core.Models.Vo;

public partial class LeafVo : ObservableObject
{
    public string Key { get; private set; }
    public bool IsChanged { get; set; }
    public GameValueType Type { get; private set; }

    [ObservableProperty]
    private string _value;

    public LeafVo(string key, Types.Value value)
    {
        Key = key;
        _value = value.ToRawString();
        Type = GetValue(value);
    }

    private static GameValueType GetValue(Types.Value value)
    {
        if (value.IsBool)
        {
            return GameValueType.Bool;
        }

        if (value.IsFloat)
        {
            return GameValueType.Float;
        }

        if (value.IsInt)
        {
            return GameValueType.Int;
        }

        if (value.IsString)
        {
            return GameValueType.String;
        }

        if (value.IsQString)
        {
            return GameValueType.StringWithQuotation;
        }

        // if (value.IsClause)
        // {
        //     return GameValueType.Clause;
        // }
        throw new InvalidEnumArgumentException(nameof(value));
    }

    protected override void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        IsChanged = true;
        base.OnPropertyChanging(e);
    }

    public Types.Value ToRawValue()
    {
	    return Type switch
	    {
		    GameValueType.Bool => Types.Value.NewBool(bool.Parse(Value)),
		    GameValueType.Float => Types.Value.NewFloat(decimal.Parse(Value)),
		    GameValueType.Int => Types.Value.NewInt(int.Parse(Value)),
		    GameValueType.String => Types.Value.NewStringValue(Value),
		    GameValueType.StringWithQuotation => Types.Value.NewQStringValue(Value),
		    GameValueType.None => throw new ArgumentException(),
		    _ => throw new ArgumentException()
	    };
    }
}
