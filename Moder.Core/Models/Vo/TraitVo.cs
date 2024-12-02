using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Moder.Core.Messages;
using Moder.Core.Models.Character;
using Moder.Core.Parser;
using Moder.Core.Services;
using Moder.Core.Services.GameResources;
using Moder.Language.Strings;

namespace Moder.Core.Models.Vo;

public sealed partial class TraitVo(Trait trait, string localisationName)
    : ObservableObject,
        IEquatable<TraitVo>
{
    public string Name => trait.Name;
    public Trait Trait => trait;
    public string LocalisationName => localisationName;
    public TextBlock Description => GetDescription();

    private static readonly ModifierService ModifierService =
        App.Current.Services.GetRequiredService<ModifierService>();
    private static readonly LocalisationService LocalisationService =
        App.Current.Services.GetRequiredService<LocalisationService>();

    private static readonly string Separator = new('-', 25);

    /// <summary>
    /// 是否已选择, 当值改变时, 发送 <see cref="SelectedTraitChangedMessage"/> 通知
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    partial void OnIsSelectedChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new SelectedTraitChangedMessage(value, this));
    }

    private TextBlock GetDescription()
    {
        var textBox = new TextBlock();
        foreach (var inline in ModifierService.GetModifierInlines(trait.AllModifiers))
        {
            textBox.Inlines.Add(inline);
        }

        if (textBox.Inlines.Count == 0)
        {
            textBox.Inlines.Add(new Run { Text = Resource.ModifierDisplay_Empty });
        }

        textBox.Inlines.Add(new LineBreak());
        textBox.Inlines.Add(new Run { Text = Separator });
        textBox.Inlines.Add(new LineBreak());

        var traitDesc = LocalisationService.GetValue($"{trait.Name}_desc");

        foreach (var chars in GetCleanText(traitDesc).Chunk(15))
        {
            textBox.Inlines.Add(new Run { Text = new string(chars) });
            textBox.Inlines.Add(new LineBreak());
        }

        if (textBox.Inlines[^1] is LineBreak)
        {
            textBox.Inlines.RemoveAt(textBox.Inlines.Count - 1);
        }
        return textBox;
    }

    private static string GetCleanText(string rawText)
    {
        string text;
        if (LocalizationFormatParser.TryParse(rawText, out var formats))
        {
            var sb = new StringBuilder();
            foreach (var format in formats)
            {
                sb.Append(
                    format.Type == LocalizationFormatType.TextWithColor ? format.Text[1..] : format.Text
                );
            }
            text = sb.ToString();
        }
        else
        {
            text = rawText;
        }

        return text;
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
