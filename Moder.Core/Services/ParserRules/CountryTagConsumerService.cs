using MethodTimer;

namespace Moder.Core.Services.ParserRules;

public class CountryTagConsumerService
{
    private readonly string[] _keywords;
    private const string FileName = "CountryTag.txt";

    public CountryTagConsumerService()
    {
        var configFilePath = Path.Combine(App.ParserRulesFolder, FileName);
        if (!File.Exists(configFilePath))
        {
            _keywords = [];
            return;
        }

        _keywords = ReadKeywordsInFile(configFilePath);
    }

    [Time("加载解析规则 CountryTag.txt")]
    private static string[] ReadKeywordsInFile(string configFilePath)
    {
        var lines = File.ReadAllLines(configFilePath);
        return lines.Select(keyword => keyword.Trim()).ToArray();
    }

    /// <summary>
    /// 检查 <c>keyword</c> 是否是使用 Country Tag 的关键字
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns></returns>
    public bool IsKeyword(string keyword)
    {
        return Array.FindIndex(_keywords, k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase)) != -1;
    }
}
