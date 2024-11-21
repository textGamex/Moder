using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Moder.Core.Messages;
using Moder.Core.Models.Character;
using Moder.Core.Services;

namespace Moder.Core.Models.Vo;

public sealed partial class TraitVo(Trait trait, string localisationName) : ObservableObject, IEquatable<TraitVo>
{
    public string Name => trait.Name;
    public Trait Trait => trait;
    public string LocalisationName => localisationName;
    public TextBlock Description => GetDescription();

    [ObservableProperty]
    private bool _isSelected;

    partial void OnIsSelectedChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new SelectedTraitChangedMessage(value, this));
    }

    private TextBlock GetDescription()
    {
        var textBox = new TextBlock();
        foreach (
            var inline in App
                .Current.Services.GetRequiredService<ModifierService>()
                .GetModifierInlines(trait.AllModifiers)
        )
        {
            textBox.Inlines.Add(inline);
        }

        if (textBox.Inlines.Count == 0)
        {
            textBox.Inlines.Add(new Run { Text = "无修正效果" });
        }

        return textBox;
    }

    public bool Equals(TraitVo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TraitVo);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}