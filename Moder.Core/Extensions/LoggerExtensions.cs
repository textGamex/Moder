using Microsoft.Extensions.Logging;
using ParadoxPower.CSharp;

namespace Moder.Core.Extensions;

public static class LoggerExtensions
{
	public static void LogParseError(this ILogger logger, ParserError error)
	{
		logger.LogWarning("文件解析失败, 原因: {Message}, path: {Path}", error.ErrorMessage, error.Filename);
	}
}