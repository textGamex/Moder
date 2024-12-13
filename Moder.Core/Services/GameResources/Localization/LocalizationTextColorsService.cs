using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Media;
using MethodTimer;
using Moder.Core.Models.Game;
using Moder.Core.Services.GameResources.Base;
using ParadoxPower.Process;

namespace Moder.Core.Services.GameResources.Localization;

[Time("加载本地化文本颜色")]
public sealed class LocalizationTextColorsService()
    : CommonResourcesService<LocalizationTextColorsService, FrozenDictionary<char, LocalizationTextColor>>(
        Path.Combine("interface", WatcherFilter.InterfaceCoreGfxFile.Name),
        WatcherFilter.InterfaceCoreGfxFile,
        PathType.File
    )
{
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
            var colorBytes = new List<byte>(3);

            foreach (var leafValue in textColorNode.LeafValues)
            {
                if (byte.TryParse(leafValue.ValueText, out var colorByte))
                {
                    colorBytes.Add(colorByte);
                }
                else
                {
                    Log.Warn("颜色 {Key} 的值 {Value} 不是数字", textColorNode.Key, colorByte);
                }
            }
            if (colorBytes.Count != 3)
            {
                Log.Warn("颜色 {Key} 的长度不正确", textColorNode.Key);
                continue;
            }

            colors[key] = new LocalizationTextColor(
                key,
                Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2])
            );
        }

        return colors.ToFrozenDictionary();
    }
}
