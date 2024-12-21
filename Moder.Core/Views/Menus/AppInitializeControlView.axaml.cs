﻿using System.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using EnumsNET;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Extensions;
using Moder.Core.Models;
using Moder.Core.Services.Config;
using AppInitializeControlViewModel = Moder.Core.ViewsModel.Menus.AppInitializeControlViewModel;

namespace Moder.Core.Views.Menus;

public sealed partial class AppInitializeControlView : UserControl
{
    private IDisposable? _selectFolderInteractionDisposable;

    public AppInitializeControlView()
    {
        InitializeComponent();

        var viewModel = App.Services.GetRequiredService<AppInitializeControlViewModel>();
        DataContext = viewModel;
        ThemeSelector.SelectionChanged += ThemeSelectorOnSelectionChanged;
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

        App.Current.RequestedThemeVariant = theme.ToThemeVariant();
        var settingService = App.Services.GetRequiredService<AppSettingService>();
        settingService.AppTheme = theme;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        _selectFolderInteractionDisposable?.Dispose();

        if (DataContext is AppInitializeControlViewModel viewModel)
        {
            _selectFolderInteractionDisposable = viewModel.SelectFolderInteraction.RegisterHandler(Handler);
        }

        base.OnDataContextChanged(e);
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
