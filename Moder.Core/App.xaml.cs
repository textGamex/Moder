using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Moder.Core;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
	public new static App Current => (App)Application.Current;

	public IServiceProvider Services => Current._serviceProvider;
	public MainWindow MainWindow { get; private set; } = null!;
	public static string ConfigFolder { get; } = Path.Combine(Environment.CurrentDirectory, "Configs");

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

		Initialize();
	}

	private void Initialize()
	{
		if (!Directory.Exists(ConfigFolder))
		{
			Directory.CreateDirectory(ConfigFolder);
		}
	}

	/// <summary>
	/// Invoked when the application is launched.
	/// </summary>
	/// <param name="args">Details about the launch request and process.</param>
	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
		MainWindow.Activate();
	}
}