using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Moder.Core.Extensions;

public static class MethodTimeLogger
{
	private static readonly ILogger<App> Logger = App.Current.Services.GetRequiredService<ILogger<App>>();

	public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
	{
		Logger.LogDebug("{Message} 耗时: {Time:F3} ms {Name}", message, elapsed.TotalMilliseconds, methodBase.Name);
	}
}