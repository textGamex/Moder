using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Moder.Core.Infrastructure.Parser;
using Moder.Core.Models.Game.Character;

namespace Moder.Core.Models.Vo;

public sealed partial class TraitVo : ObservableObject, IEquatable<TraitVo>
{
    public string Name => Trait.Name;
    public Trait Trait { get; }

    public string LocalisationName { get; }

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

    // private ImageSource? GetImageSource()
    // {
    //     if (SpriteService.TryGetImageSource($"GFX_trait_{Name}", out var source))
    //     {
    //         return source;
    //     }
    //
    //     return UnknownImage;
    // }

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
