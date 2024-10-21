using System.Reflection;
using NLog;

namespace Moder.Core.Extensions;

public static class MethodTimeLogger
{
	private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

	public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
	{
		Logger.Debug("{Message} 耗时: {Time:F3} ms {Name}", message, elapsed.TotalMilliseconds, methodBase.Name);
	}
}