using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moder.Core.Services;
using Moder.Core.Services.Config;
using Moder.Core.Views;
using Moder.Core.ViewsModel;
using Moder.Hosting;
using NLog;
using NLog.Extensions.Logging;

namespace Moder.Core;

public class App : Application
{
    public const string AppVersion = "0.1.0-alpha";
    public static new App Current => (App)Application.Current!;
    public IServiceProvider Services => Current._serviceProvider;
    public static string AppConfigFolder { get; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Moder",
            "Configs"
        );
    public static string ParserRulesFolder { get; } =
        Path.Combine(Environment.CurrentDirectory, "Assets", "ParserRules");

    private IHost? _host;

    public App()
    {
        InitializeApp();
    }

    private static void InitializeApp()
    {
        if (!Directory.Exists(AppConfigFolder))
        {
            Directory.CreateDirectory(AppConfigFolder);
        }

        if (!Directory.Exists(ParserRulesFolder))
        {
            Directory.CreateDirectory(ParserRulesFolder);
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private IServiceProvider _serviceProvider = null!;

    public override async void OnFrameworkInitializationCompleted()
    {
        var builder = CreateHostBuilder();
        var host = builder.Build();
        _host = host;
        _serviceProvider = host.Services;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            desktop.Exit += (_, _) =>
            {
                _serviceProvider.GetRequiredService<AppSettingService>().SaveChanged();
                _host.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
                _host.Dispose();
                _host = null;

                // 在退出时刷新日志
                LogManager.Flush();
            };
        }

        base.OnFrameworkInitializationCompleted();

        await _host.RunAsync();
    }

    private static HostApplicationBuilder CreateHostBuilder()
    {
        var settings = new HostApplicationBuilderSettings
        {
            Args = Environment.GetCommandLineArgs(),
            ApplicationName = "Moder"
        };

#if DEBUG
        settings.EnvironmentName = "Development";
#else
        settings.EnvironmentName = "Production";
#endif
        var builder = Host.CreateApplicationBuilder(settings);

        builder.Services.AttachLoggerToAvaloniaLogger();
        builder.Logging.ClearProviders();
        builder.Logging.AddNLog(builder.Configuration);
        LogManager.Configuration = new NLogLoggingConfiguration(builder.Configuration.GetSection("NLog"));

        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<MainWindowViewModel>();
        builder.Services.AddTransient<AppInitializeControlView>();
        builder.Services.AddTransient<AppInitializeControlViewModel>();

        builder.Services.AddSingleton(_ => AppSettingService.Load());
        builder.Services.AddSingleton<MessageBoxService>();

        return builder;
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove = BindingPlugins
            .DataValidators.OfType<DataAnnotationsValidationPlugin>()
            .ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
