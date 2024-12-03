using System.Collections.Frozen;
using MethodTimer;
using Moder.Core.Extensions;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.Process;

namespace Moder.Core.Services.GameResources;

public sealed class SpriteService : CommonResourcesService<SpriteService, FrozenDictionary<string, string>>
{
    [Time("加载界面图片")]
    public SpriteService()
        : base("interface", WatcherFilter.GfxFiles) { }

    protected override FrozenDictionary<string, string>? ParseFileToContent(Node rootNode)
    {
        var sprites = new Dictionary<string, string>(16);

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

    private static void ParseSpriteTypeNodeToDictionary(Node spriteTypeNode, Dictionary<string, string> sprites)
    {
        string? spriteTypeName = null;
        string? textureFile = null;
        foreach (var leaf in spriteTypeNode.Leaves)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals("name", leaf.Key))
            {
                spriteTypeName = leaf.ValueText;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals("texturefile", leaf.Key))
            {
                textureFile = leaf.ValueText;
            }
        }

        if (spriteTypeName is null || textureFile is null)
        {
            return;
        }

        sprites[spriteTypeName] = textureFile;
    }
}
