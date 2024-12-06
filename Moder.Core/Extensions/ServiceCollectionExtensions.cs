using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Moder.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddViewSingleton<TView, TViewModel>(this IServiceCollection services)
        where TView : Control, new()
        where TViewModel : class
    {
        services.AddSingleton<TView>();
        services.AddSingleton<TViewModel>();
    }

    public static void AddViewTransient<TView, TViewModel>(this IServiceCollection services)
        where TView : Control, new()
        where TViewModel : class
    {
        services.AddTransient<TView>();
        services.AddTransient<TViewModel>();
    }
}
