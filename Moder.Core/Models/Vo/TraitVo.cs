using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Moder.Core.Infrastructure.Parser;
using Moder.Core.Models.Game.Character;

namespace Moder.Core.Models.Vo;

public sealed partial class TraitVo : ObservableObject,
        IEquatable<TraitVo>
{
    public string Name => Trait.Name;
    public Trait Trait { get; }

    public string LocalisationName { get; }

    // public TextBlock Description => GetDescription();
    //
    // private static readonly ModifierService ModifierService =
    //     App.Current.Services.GetRequiredService<ModifierService>();
    // private static readonly LocalisationService LocalisationService =
    //     App.Current.Services.GetRequiredService<LocalisationService>();
    // private static readonly SpriteService SpriteService =
    //     App.Current.Services.GetRequiredService<SpriteService>();
    //
    // private static readonly string Separator = new('-', 25);
    // private static readonly ImageSource? UnknownImage = GetUnknownImageSource();

    // private static ImageSource? GetUnknownImageSource()
    // {
    //     if (SpriteService.TryGetImageSource("GFX_trait_unknown", out var source))
    //     {
    //         return source;
    //     }
    //
    //     return null;
    // }

    /// <summary>
    /// 是否已选择, 当值改变时, 发送 <see cref="SelectedTraitChangedMessage"/> 通知
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public TraitVo(Trait trait, string localisationName)
    {
        Trait = trait;
        LocalisationName = localisationName;
        // _imageSource = new Lazy<ImageSource?>(GetImageSource);
    }

    // public ImageSource? ImageSource => _imageSource.Value;
    // private readonly Lazy<ImageSource?> _imageSource;

    // partial void OnIsSelectedChanged(bool value)
    // {
    //     WeakReferenceMessenger.Default.Send(new SelectedTraitChangedMessage(value, this));
    // }

    // private ImageSource? GetImageSource()
    // {
    //     if (SpriteService.TryGetImageSource($"GFX_trait_{Name}", out var source))
    //     {
    //         return source;
    //     }
    //
    //     return UnknownImage;
    // }

    // private TextBlock GetDescription()
    // {
    //     var textBox = new TextBlock();
    //     foreach (var inline in ModifierService.GetModifierInlines(Trait.AllModifiers))
    //     {
    //         textBox.Inlines.Add(inline);
    //     }
    //
    //     if (textBox.Inlines.Count == 0)
    //     {
    //         textBox.Inlines.Add(new Run { Text = Resource.ModifierDisplay_Empty });
    //     }
    //
    //     textBox.Inlines.Add(new LineBreak());
    //     textBox.Inlines.Add(new Run { Text = Separator });
    //     textBox.Inlines.Add(new LineBreak());
    //
    //     var traitDesc = LocalisationService.GetValue($"{Trait.Name}_desc");
    //
    //     foreach (var chars in GetCleanText(traitDesc).Chunk(15))
    //     {
    //         textBox.Inlines.Add(new Run { Text = new string(chars) });
    //         textBox.Inlines.Add(new LineBreak());
    //     }
    //
    //     if (textBox.Inlines[^1] is LineBreak)
    //     {
    //         textBox.Inlines.RemoveAt(textBox.Inlines.Count - 1);
    //     }
    //     return textBox;
    // }

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