using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Moder.Core.Infrastructure;
using Moder.Core.Messages;
using Moder.Core.Resources;
using Moder.Core.Services;
using Moder.Core.Services.Config;
using Moder.Language.Strings;
using NLog;

namespace Moder.Core.ViewsModel.Menus;

public sealed partial class AppInitializeControlViewModel(
    AppSettingService settingService,
    MessageBoxService messageBox
) : ObservableValidator
{
    [Required(
        ErrorMessageResourceName = "UIErrorMessage_Required",
        ErrorMessageResourceType = typeof(Resource)
    )]
    [ObservableProperty]
    private string _gameRootFolderPath = string.Empty;

    [Required(
        ErrorMessageResourceName = "UIErrorMessage_Required",
        ErrorMessageResourceType = typeof(Resource)
    )]
    [ObservableProperty]
    private string _modRootFolderPath = string.Empty;

    public Interaction<string, string> SelectFolderInteraction { get; } = new();
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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

    [RelayCommand]
    private async Task Submit()
    {
        ValidateAllProperties();
        if (string.IsNullOrEmpty(GameRootFolderPath) || string.IsNullOrEmpty(ModRootFolderPath))
        {
            await messageBox.WarnAsync(Resource.UIErrorMessage_MissingRequiredInfoTip);
            return;
        }

        settingService.GameRootFolderPath = GameRootFolderPath;
        settingService.ModRootFolderPath = ModRootFolderPath;
        Log.Info("资源目录设置成功");

        WeakReferenceMessenger.Default.Send(new CompleteAppInitializeMessage());
    }

    public Type AppThemes { get; } = typeof(ThemeMode);
}
