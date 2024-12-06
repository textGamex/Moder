using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.ViewsModel;
using AppInitializeControlViewModel = Moder.Core.ViewsModel.Menus.AppInitializeControlViewModel;

namespace Moder.Core.Views.Menus;

public partial class AppInitializeControlView : UserControl
{
    private IDisposable? _selectFolderInteractionDisposable;

    public AppInitializeControlView()
    {
        InitializeComponent();

        var viewModel = App.Services.GetRequiredService<AppInitializeControlViewModel>();
        DataContext = viewModel;
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
