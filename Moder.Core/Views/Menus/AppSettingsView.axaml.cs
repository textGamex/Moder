using System.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using EnumsNET;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Extensions;
using Moder.Core.Infrastructure;
using Moder.Core.Models;
using Moder.Core.Services.Config;
using Moder.Core.ViewsModel.Game;
using Moder.Core.ViewsModel.Menus;
using Moder.Language.Strings;

namespace Moder.Core.Views.Menus;

public partial class AppSettingsView : UserControl, ITabViewItem
{
    private IDisposable? _selectFolderInteractionDisposable;
    
    public AppSettingsView()
    {
        InitializeComponent();
        ViewModel= App.Services.GetRequiredService<AppSettingsViewModel>();
        DataContext = ViewModel;
        ThemeSelector.SelectionChanged += ThemeSelectorOnSelectionChanged;
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
        var themeType = settings.AppTheme;
        var names = Enums.GetNames(typeof(ThemeMode));
        var name = $"{nameof(ThemeMode)}.{themeType}";
        var resourceManager = new ResourceManager(typeof(Language.Strings.Resource));
        ThemeSelector.SelectedItem =
            resourceManager.GetString(name, Language.Strings.Resource.Culture)
            ?? Language.Strings.Resource.LocalizeValueNotFind;
        GameRootSelector.DirectoryPath = settings.GameRootFolderPath;
        ModRootSelector.DirectoryPath = settings.ModRootFolderPath;
    }

    private void ThemeSelectorOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var type = typeof(ThemeMode);
        var names = Enums.GetNames(type);
        var index = ThemeSelector.SelectedIndex;
        if (index >= names.Count || index < 0)
        {
            return;
        }
        var obj = names[index].ToEnum(type);
        if (obj is not ThemeMode theme)
        {
            return;
        }
        var app = Application.Current;
        if (app is null)
        {
            return;
        }
        app.RequestedThemeVariant = theme.ToThemeVariant();
        var settingService = App.Services.GetRequiredService<AppSettingService>();
        settingService.AppTheme = theme;
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