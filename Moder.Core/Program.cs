using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moder.Core.Services;
using Moder.Core.Services.Config;
using Moder.Core.Services.GameResources;
using Moder.Core.Services.ParserRules;
using Moder.Core.Views;
using Moder.Core.Views.Game;
using Moder.Core.Views.Menus;
using Moder.Core.ViewsModels;
using Moder.Core.ViewsModels.Game;
using Moder.Core.ViewsModels.Menus;
using Moder.Hosting.WinUI;
using NLog.Extensions.Logging;

namespace Moder.Core;

#if !DISABLE_XAML_GENERATED_MAIN
#error "This project only works with custom Main entry point. Must set DISABLE_XAML_GENERATED_MAIN to True."
#endif

public static partial class Program
{
    /// <summary>
    /// Ensures that the process can run XAML, and provides a deterministic error if a
    /// check fails. Otherwise, it quietly does nothing.
    /// </summary>
    [LibraryImport("Microsoft.ui.xaml.dll")]
    private static partial void XamlCheckProcessRequirements();

    [STAThread]
    private static void Main(string[] args)
    {
        XamlCheckProcessRequirements();

        var builder = Host.CreateApplicationBuilder(args);

        // View, ViewModel
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<MainWindowViewModel>();
        builder.Services.AddTransient<OpenFolderControlView>();
        builder.Services.AddTransient<OpenFolderControlViewModel>();
        builder.Services.AddSingleton<SideWorkSpaceControlView>();
        builder.Services.AddSingleton<SideWorkSpaceControlViewModel>();
        builder.Services.AddTransient<StateFileControlView>();
        builder.Services.AddTransient<StateFileControlViewModel>();
        builder.Services.AddTransient<NotSupportInfoControlView>();
        builder.Services.AddTransient<SettingsControlView>();
        builder.Services.AddTransient<SettingsControlViewModel>();
        builder.Services.AddTransient<CharacterEditorControlView>();
        builder.Services.AddTransient<CharacterEditorControlViewModel>();
        builder.Services.AddTransient<TraitsSelectionWindowView>();
        builder.Services.AddTransient<TraitsSelectionWindowViewModel>();

        builder.Logging.ClearProviders();
        builder.Logging.AddNLog(builder.Configuration);
        NLog.LogManager.Configuration = new NLogLoggingConfiguration(
            builder.Configuration.GetSection("NLog")
        );

        builder.Services.AddSingleton<MessageBoxService>();
        builder.Services.AddSingleton<GlobalSettingService>(_ => GlobalSettingService.Load());
        builder.Services.AddSingleton<GlobalResourceService>();

        builder.Services.AddSingleton<LeafConverterService>();
        builder.Services.AddSingleton<CountryTagConsumerService>();
        builder.Services.AddSingleton<GameResourcesWatcherService>();
        builder.Services.AddSingleton<GameResourcesPathService>();
        builder.Services.AddSingleton<ModifierService>();
        builder.Services.AddSingleton<LocalisationKeyMappingService>();

        builder.Services.AddSingleton<GameModDescriptorService>();
        // 游戏内资源
        builder.Services.AddSingleton<CharacterSkillService>();
        builder.Services.AddSingleton<CharacterTraitsService>();
        builder.Services.AddSingleton<TerrainService>();
        builder.Services.AddSingleton<StateCategoryService>();
        builder.Services.AddSingleton<LocalisationService>();
        builder.Services.AddSingleton<OreService>();
        builder.Services.AddSingleton<BuildingsService>();
        builder.Services.AddSingleton<CountryTagService>();

        // Setup and provision the hosting context for the User Interface
        // service.
        ((IHostApplicationBuilder)builder).Properties.Add(
            HostingExtensions.HostingContextKey,
            new HostingContext() { IsLifetimeLinked = true }
        );

        // Add the WinUI User Interface hosted service as early as possible to
        // allow the UI to start showing up while you continue setting up other
        // services not required for the UI.
        var host = builder.ConfigureWinUI<App>().Build();

        // Finally start the host. This will block until the application
        // lifetime is terminated through CTRL+C, closing the UI windows or
        // programmatically.
        host.Run();
    }
}
