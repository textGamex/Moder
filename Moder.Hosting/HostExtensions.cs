using System.Runtime.Versioning;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Moder.Hosting;

public static class HostExtensions
{
    [SupportedOSPlatform("macos")]
    public static int RunAvaloniaAppInMacos(this IHost host)
    {
        var lifetime = host.Services.GetRequiredService<IHostedLifetime>();
        var application = host.Services.GetRequiredService<Application>();
        var result = host.StartAsync(CancellationToken.None)
            .ContinueWith(_ =>
                lifetime.StartAsync(application, CancellationToken.None).GetAwaiter().GetResult()
            )
            .GetAwaiter()
            .GetResult();

        Task.WaitAll(
            host.StopAsync(CancellationToken.None),
            host.WaitForShutdownAsync(CancellationToken.None)
        );

        return result;
    }

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    public static async Task<int> RunAvaloniaAppAsync(this IHost host, CancellationToken token = default)
    {
        var lifetime = host.Services.GetRequiredService<IHostedLifetime>();
        var application = host.Services.GetRequiredService<Application>();
        await host.StartAsync(token);
        var result = await lifetime.StartAsync(application, token);

        await host.StopAsync(token);

        await host.WaitForShutdownAsync(token);

        return result;
    }
}
