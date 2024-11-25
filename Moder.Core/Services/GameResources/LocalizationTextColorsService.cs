using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using MethodTimer;
using Moder.Core.Models;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.Process;
using Windows.UI;

namespace Moder.Core.Services.GameResources;

public sealed class LocalizationTextColorsService
    : CommonResourcesService<LocalizationTextColorsService, FrozenDictionary<char, LocalizationTextColor>>
{
    [Time("加载本地化文本颜色")]
    public LocalizationTextColorsService()
        : base(
            Path.Combine("interface", WatcherFilter.InterfaceCoreGfxFile.Name),
            WatcherFilter.InterfaceCoreGfxFile,
            PathType.File
        ) { }

    public bool TryGetColor(char key, [NotNullWhen(true)] out LocalizationTextColor? color)
    {
        foreach (var map in Resources.Values)
        {
            if (map.TryGetValue(key, out color))
            {
                return true;
            }
        }

        color = null;
        return false;
    }

    protected override FrozenDictionary<char, LocalizationTextColor>? ParseFileToContent(Node rootNode)
    {
        var bitmapFontsNode = rootNode.Nodes.FirstOrDefault(node =>
            StringComparer.OrdinalIgnoreCase.Equals("bitmapfonts", node.Key)
        );

        var textColorsNode = bitmapFontsNode?.Nodes.FirstOrDefault(node =>
            StringComparer.OrdinalIgnoreCase.Equals("textcolors", node.Key)
        );

        if (textColorsNode is null)
        {
            return null;
        }

        var colors = new Dictionary<char, LocalizationTextColor>(textColorsNode.AllArray.Length);
        foreach (var textColorNode in textColorsNode.Nodes)
        {
            var key = textColorNode.Key[0];
            var color = textColorNode
                .LeafValues.Select(value => byte.TryParse(value.ValueText, out var result) ? result : (byte)0)
                .ToArray();

            if (color.Length != 3)
            {
                Logger.Warn("颜色 {Key} 的长度不正确", textColorNode.Key);
                continue;
            }

            colors[key] = new LocalizationTextColor(key, Color.FromArgb(255, color[0], color[1], color[2]));
        }

        return colors.ToFrozenDictionary();
    }
}
