using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Extensions;
using Moder.Core.Helper;
using Moder.Core.Services.GameResources;
using ParadoxPower.Parser;

namespace Moder.Core.Models.Vo;

public partial class LeafVo : ObservableGameValue
{
    public virtual string Value
    {
        get => LeafValue;
        set => SetProperty(ref this.LeafValue, value);
    }
    protected string LeafValue;

    protected static readonly LocalisationService LocalisationService;
    protected static readonly GameResourcesService GameResourcesService;

    static LeafVo()
    {
        GameResourcesService = App.Current.Services.GetRequiredService<GameResourcesService>();
        LocalisationService = GameResourcesService.Localisation;
    }

    public LeafVo(string key, Types.Value value, NodeVo? parent)
        : base(key, parent)
    {
        LeafValue = value.ToRawString();
        Type = value.ToLocalValueType();
    }

    public Types.Value ToRawValue()
    {
        return ValueConverterHelper.ToValueType(Type, Value);
    }
}
