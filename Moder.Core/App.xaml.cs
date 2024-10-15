using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Moder.Core.Services.Config;

namespace Moder.Core;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public const string AppVersion = "0.1.0-alpha";
    public static new App Current => (App)Application.Current;

    public IServiceProvider Services => Current._serviceProvider;
    public Views.MainWindow MainWindow { get; private set; } = null!;
    public static string ConfigFolder { get; } = Path.Combine(Environment.CurrentDirectory, "Configs");
    public static string ParserRulesFolder { get; } = Path.Combine(ConfigFolder, "ParserRules");

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App(IServiceProvider serviceProvider, ILogger<App> logger)
    {
        _serviceProvider = serviceProvider;
        UnhandledException += (sender, args) => logger.LogError(args.Exception, "Unhandled exception");
        InitializeComponent();

        InitializeApp();
    }

    private static void InitializeApp()
    {
        if (!Directory.Exists(ConfigFolder))
        {
            Directory.CreateDirectory(ConfigFolder);
        }

        if (!Directory.Exists(ParserRulesFolder))
        {
            Directory.CreateDirectory(ParserRulesFolder);
        }
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = _serviceProvider.GetRequiredService<Views.MainWindow>();
        SetAppTheme();
        MainWindow.Activate();
    }

    private void SetAppTheme()
    {
        Debug.Assert(MainWindow is not null);

        var settings = _serviceProvider.GetRequiredService<GlobalSettingService>();
        if (MainWindow.Content is FrameworkElement root)
        {
            root.RequestedTheme = settings.AppThemeMode;
        }
    }
}
