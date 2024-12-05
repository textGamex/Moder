using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Moder.Core.ViewsModel;

namespace Moder.Core.Views;

public partial class AppInitializeControlView : UserControl
{
    public AppInitializeControlView(AppInitializeControlViewModel viewModel)
    {
        //TODO: 释放
        InitializeComponent();

        DataContext = viewModel;
        viewModel.SelectFolderInteraction.RegisterHandler(Handler);
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
