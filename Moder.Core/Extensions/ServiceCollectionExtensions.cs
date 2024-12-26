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
        if (
            typeof(IDisposable).IsAssignableFrom(typeof(TView))
            || typeof(IDisposable).IsAssignableFrom(typeof(TViewModel))
        )
        {
            throw new InvalidOperationException("使用 Transient 注入时不能继承 IDisposable 接口");
        }
        services.AddTransient<TView>();
        services.AddTransient<TViewModel>();
    }
}
