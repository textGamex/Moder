using Microsoft.Extensions.Logging;
using ParadoxPower.CSharp;

namespace Moder.Core.Extensions;

public static class LoggerExtends
{
	public static void LogParseError(this ILogger logger, ParserError error)
	{
		logger.LogError("解析错误, 原因: {Message}, path: {Path}", error.ErrorMessage, error.Filename);
	}
}