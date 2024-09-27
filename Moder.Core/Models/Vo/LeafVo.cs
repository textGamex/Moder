﻿using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Extensions;
using Moder.Core.Helper;
using Moder.Core.Services;
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

    protected static readonly LocalisationService LocalisationService = App
        .Current.Services.GetRequiredService<GameResourcesService>()
        .Localisation;

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
