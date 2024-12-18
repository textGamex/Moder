using System.Diagnostics;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moder.Core.Extensions;
using Moder.Core.Infrastructure.FileSort;
using Moder.Core.Services;
using Moder.Core.Services.Config;
using Moder.Core.Services.FileNativeService;
using Moder.Core.Services.GameResources;
using Moder.Core.Services.GameResources.Base;
using Moder.Core.Services.GameResources.Localization;
using Moder.Core.Services.GameResources.Modifiers;
using Moder.Core.Views;
using Moder.Core.Views.Game;
using Moder.Core.Views.Menus;
using Moder.Core.ViewsModel;
using Moder.Core.ViewsModel.Game;
using Moder.Core.ViewsModel.Menus;
using Moder.Hosting;
using NLog;
using NLog.Extensions.Logging;

namespace Moder.Core;

public class App : Application
{
    public const string AppVersion = "0.1.0-alpha";
    public static new App Current => (App)Application.Current!;
    public static IServiceProvider Services => Current._serviceProvider;
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
        this.EnableHotReload();
        AvaloniaXamlLoader.Load(this);
    }

    private IServiceProvider _serviceProvider = null!;

    public override async void OnFrameworkInitializationCompleted()
    {
        var builder = CreateHostBuilder();
        var host = builder.Build();
        _host = host;
        _serviceProvider = host.Services;
        var settingService = Services.GetRequiredService<AppSettingService>();
        RequestedThemeVariant = settingService.AppTheme.ToThemeVariant();

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

        // View, ViewModel
        builder.Services.AddViewSingleton<MainWindow, MainWindowViewModel>();
        builder.Services.AddViewTransient<AppInitializeControlView, AppInitializeControlViewModel>();
        builder.Services.AddViewSingleton<MainControlView, MainControlViewModel>();
        builder.Services.AddViewSingleton<SideBarControlView, SideBarControlViewModel>();
        builder.Services.AddViewSingleton<WorkSpaceControlView, WorkSpaceControlViewModel>();
        builder.Services.AddViewTransient<CharacterEditorControlView, CharacterEditorControlViewModel>();
        builder.Services.AddViewTransient<AppSettingsView, AppSettingsViewModel>();
        builder.Services.AddTransient<TraitSelectionWindowViewModel>();

        builder.Services.AddSingleton(_ => AppSettingService.Load());
        builder.Services.AddSingleton<MessageBoxService>();
        builder.Services.AddSingleton<TabViewNavigationService>();
        builder.Services.AddSingleton<AppResourcesService>();
        builder.Services.AddSingleton<GameModDescriptorService>();
        builder.Services.AddSingleton<GameResourcesPathService>();
        builder.Services.AddSingleton<GameResourcesWatcherService>();

        // 本地化文本相关服务
        builder.Services.AddSingleton<LocalizationService>();
        builder.Services.AddSingleton<LocalizationFormatService>();
        builder.Services.AddSingleton<LocalizationTextColorsService>();
        builder.Services.AddSingleton<LocalizationKeyMappingService>();

        // 修饰符相关服务
        builder.Services.AddSingleton<ModifierService>();
        builder.Services.AddSingleton<ModifierDisplayService>();

        builder.Services.AddSingleton<CharacterSkillService>();
        builder.Services.AddSingleton<CharacterTraitsService>();
        builder.Services.AddSingleton<TerrainService>();

        AddPlatformNativeServices(builder.Services);

        return builder;
    }

    private static void AddPlatformNativeServices(IServiceCollection builder)
    {
#if WINDOWS
        Debug.Assert(OperatingSystem.IsWindows());
        AddWindowsServices(builder);
#elif LINUX
        AddLinuxServices(builder);
#endif
    }

#if WINDOWS
    [SupportedOSPlatform("windows")]
    private static void AddWindowsServices(IServiceCollection builder)
    {
        builder.AddSingleton<IFileSortComparer, WindowsFileSortComparer>();
        builder.AddSingleton<IFileNativeService, WindowsFileNativeService>();
    }
#elif LINUX
    [SupportedOSPlatform("linux")]
    private static void AddLinuxServices(IServiceCollection builder)
    {
        builder.AddSingleton<IFileSortComparer, LinuxFileSortComparer>();
        builder.AddSingleton<IFileNativeService, LinuxFileNativeService>();
    }
#endif

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
