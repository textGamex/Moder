using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Moder.Core.Core;
using Moder.Core.Services.Config;

namespace Moder.Core.ViewsModel;

public sealed partial class AppInitializeControlViewModel(AppSettingService settingService) : ObservableObject
{
    [ObservableProperty]
    private string _gameRootFolderPath = string.Empty;

    [ObservableProperty]
    private string _modRootFolderPath = string.Empty;
    public Interaction<string, string> SelectFolderInteraction { get; } = new();

    [RelayCommand]
    private async Task SelectGameRootFolder()
    {
        var gameRootPath = await SelectFolderInteraction.HandleAsync("选择游戏根目录");
        if (string.IsNullOrEmpty(gameRootPath))
        {
            return;
        }

        GameRootFolderPath = gameRootPath;
    }

    [RelayCommand]
    private async Task SelectModRootFolder()
    {
        var modRootPath = await SelectFolderInteraction.HandleAsync("选择Mod根目录");
        if (string.IsNullOrEmpty(modRootPath))
        {
            return;
        }

        ModRootFolderPath = modRootPath;
    }
}
