using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Moder.Core.Parser;
using Moder.Core.Services.GameResources;

namespace Moder.Core.Services;

public class LocalizationFormatService
{
    private readonly LocalizationTextColorsService _localizationTextColorsService;

    public LocalizationFormatService(LocalizationTextColorsService localizationTextColorsService)
    {
        _localizationTextColorsService = localizationTextColorsService;
    }

    public IEnumerable<Inline> GetTextWithColor(string text)
    {
        var result = new List<Inline>(4);

        if (LocalizationFormatParser.TryParse(text, out var formats))
        {
            result.AddRange(formats.Select(GetTextRun));
        }
        else
        {
            result.Add(new Run { Text = text });
        }

        return result;
    }

    /// <summary>
    /// 尝试获取 <see cref="Run"/> 文本, 并使用 <see cref="LocalizationFormat"/> 中指定的颜色, 如果颜色不存在, 则使用默认颜色
    /// </summary>
    /// <param name="format">文本格式信息</param>
    /// <returns></returns>
    public Run GetTextRun(LocalizationFormat format)
    {
        Brush? foregroundBrush = null;
        var run = new Run { Text = format.Text };

        if (format.Type == LocalizationFormatType.TextWithColor)
        {
            if (format.Text.Length == 0)
            {
                return run;
            }
            if (_localizationTextColorsService.TryGetColor(format.Text[0], out var color))
            {
                foregroundBrush = new SolidColorBrush(color.Color);
                run.Text = format.Text[1..];
            }
        }

        if (foregroundBrush is not null)
        {
            run.Foreground = foregroundBrush;
        }

        return run;
    }
}
