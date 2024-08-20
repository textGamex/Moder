using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace Moder.Core;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
	public new static App Current => (App)Application.Current;
	public MainWindow MainWindow { get; private set; } = null!;

	private readonly IServiceProvider _serviceProvider;

	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		this.InitializeComponent();
	}

	/// <summary>
	/// Invoked when the application is launched.
	/// </summary>
	/// <param name="args">Details about the launch request and process.</param>
	protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
	{
		MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
		MainWindow.Activate();
	}
}