using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.WindowsRuntime;
using MethodTimer;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Moder.Core.Extensions;
using Moder.Core.Models;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.Process;
using Pfim;

namespace Moder.Core.Services.GameResources;

public sealed class SpriteService
    : CommonResourcesService<SpriteService, FrozenDictionary<string, SpriteInfo>>
{
    private readonly GameResourcesPathService _resourcesPathService;

    [Time("加载界面图片")]
    public SpriteService(GameResourcesPathService resourcesPathService)
        : base("interface", WatcherFilter.GfxFiles)
    {
        _resourcesPathService = resourcesPathService;
    }

    private Dictionary<string, FrozenDictionary<string, SpriteInfo>>.ValueCollection Sprites =>
        Resources.Values;

    public bool TryGetSpriteInfo(string spriteTypeName, [NotNullWhen(true)] out SpriteInfo? info)
    {
        foreach (var sprite in Sprites)
        {
            if (sprite.TryGetValue(spriteTypeName, out info))
            {
                return true;
            }
        }

        info = null;
        return false;
    }

    public bool TryGetImageSource(string spriteTypeName, [NotNullWhen(true)] out ImageSource? imageSource)
    {
        imageSource = null;
        if (!TryGetSpriteInfo(spriteTypeName, out var info))
        {
            return false;
        }

        try
        {
            using var image = Pfimage.FromFile(
                _resourcesPathService.GetFilePathPriorModByRelativePath(info.Path)
            );
            //BUG: 部分图片无法正常显示
            var source = new WriteableBitmap(image.Width, image.Height);
            using var stream = source.PixelBuffer.AsStream();
            stream.Write(image.Data);
            stream.Flush();

            imageSource = source;
            return true;
        }
        catch (Exception e)
        {
            Log.Warn(e, "读取图片失败, name:{Name}, path:{Path}", info.Name, info.Path);
            return false;
        }
    }

    protected override FrozenDictionary<string, SpriteInfo>? ParseFileToContent(Node rootNode)
    {
        var sprites = new Dictionary<string, SpriteInfo>(16);

        foreach (var child in rootNode.AllArray)
        {
            if (!child.IsNodeWithKey("spriteTypes", out var spriteTypes))
            {
                continue;
            }

            foreach (
                var spriteType in spriteTypes.Nodes.Where(node =>
                    StringComparer.OrdinalIgnoreCase.Equals("spriteType", node.Key)
                )
            )
            {
                ParseSpriteTypeNodeToDictionary(spriteType, sprites);
            }
        }

        return sprites.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private static void ParseSpriteTypeNodeToDictionary(
        Node spriteTypeNode,
        Dictionary<string, SpriteInfo> sprites
    )
    {
        string? spriteTypeName = null;
        string? textureFilePath = null;
        foreach (var leaf in spriteTypeNode.Leaves)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals("name", leaf.Key))
            {
                spriteTypeName = leaf.ValueText;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals("texturefile", leaf.Key))
            {
                textureFilePath = leaf.ValueText;
            }
        }

        if (spriteTypeName is null || textureFilePath is null)
        {
            return;
        }

        sprites[spriteTypeName] = new SpriteInfo(spriteTypeName, textureFilePath);
    }
}
