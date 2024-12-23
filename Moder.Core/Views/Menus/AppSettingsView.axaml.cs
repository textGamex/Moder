using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Infrastructure;
using Moder.Core.Services.Config;
using Moder.Core.ViewsModel.Menus;
using Moder.Language.Strings;

namespace Moder.Core.Views.Menus;

public sealed partial class AppSettingsView : UserControl, ITabViewItem
{
    private IDisposable? _selectFolderInteractionDisposable;

    public AppSettingsView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<AppSettingsViewModel>();
        DataContext = ViewModel;
    }

    public string Header => Resource.Menu_Settings;
    public string Id => nameof(AppSettingsView);
    public string ToolTip => Header;

    private AppSettingsViewModel ViewModel { get; }

    protected override void OnDataContextChanged(EventArgs e)
    {
        _selectFolderInteractionDisposable?.Dispose();

        if (DataContext is AppSettingsViewModel viewModel)
        {
            _selectFolderInteractionDisposable = viewModel.SelectFolderInteraction.RegisterHandler(Handler);
        }

        base.OnDataContextChanged(e);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        var settings = App.Services.GetRequiredService<AppSettingService>();
        GameRootSelector.DirectoryPath = settings.GameRootFolderPath;
        ModRootSelector.DirectoryPath = settings.ModRootFolderPath;
    }

    private async Task<string> Handler(string title)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return string.Empty;
        }

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = title, AllowMultiple = false }
        );
        var result = folders.Count > 0 ? folders[0].TryGetLocalPath() ?? string.Empty : string.Empty;

        return result;
    }
}
