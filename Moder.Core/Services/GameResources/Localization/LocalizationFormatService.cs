using Avalonia.Media;
using Avalonia.Media.Immutable;
using Moder.Core.Infrastructure.Parser;
using Moder.Core.Models;

namespace Moder.Core.Services.GameResources.Localization;

public sealed class LocalizationFormatService(LocalizationTextColorsService localizationTextColorsService)
{
    /// <summary>
    /// 从文本中获取颜色信息, 返回的集合中不包含 <see cref="LocalizationFormatType.Placeholder"/> 类型的文本, 如果解析失败, 则统一使用黑色
    /// </summary>
    /// <param name="text">文本</param>
    /// <returns>一个集合, 包含非占位符的所有文本颜色信息</returns>
    public IReadOnlyCollection<ColorTextInfo> GetColorText(string text)
    {
        var result = new List<ColorTextInfo>(4);

        if (LocalizationFormatParser.TryParse(text, out var formats))
        {
            foreach (var format in formats)
            {
                if (format.Type != LocalizationFormatType.Placeholder)
                {
                    result.Add(GetColorText(format));
                }
            }
        }
        else
        {
            result.Add(new ColorTextInfo(text, Brushes.Black));
        }

        return result;
    }

    /// <summary>
    /// 尝试将文本解析为 <see cref="ColorTextInfo"/>, 并使用 <see cref="LocalizationFormatInfo"/> 中指定的颜色, 如果颜色不存在, 则使用默认颜色
    /// </summary>
    /// <param name="format">文本格式信息</param>
    /// <returns></returns>
    public ColorTextInfo GetColorText(LocalizationFormatInfo format)
    {
        if (format.Type == LocalizationFormatType.TextWithColor)
        {
            if (string.IsNullOrEmpty(format.Text))
            {
                return new ColorTextInfo(string.Empty, Brushes.Black);
            }

            if (localizationTextColorsService.TryGetColor(format.Text[0], out var colorInfo))
            {
                if (!_colorBrushes.TryGetValue(format.Text[0], out var brush))
                {
                    brush = new ImmutableSolidColorBrush(colorInfo.Color);
                    _colorBrushes.Add(format.Text[0], brush);
                }
                return new ColorTextInfo(format.Text[1..], brush);
            }
        }

        return new ColorTextInfo(format.Text, Brushes.Black);
    }

    private readonly Dictionary<char, IImmutableSolidColorBrush> _colorBrushes = [];
}
