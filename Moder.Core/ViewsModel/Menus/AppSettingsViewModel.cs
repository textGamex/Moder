using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Infrastructure;
using Moder.Core.Messages;
using Moder.Core.Models;
using Moder.Core.Services;
using Moder.Core.Services.Config;
using Moder.Language.Strings;
using NLog;

namespace Moder.Core.ViewsModel.Menus;

public partial class AppSettingsViewModel : ObservableValidator
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
        var gameRootPath = await SelectFolderInteraction.HandleAsync(Resource.InitializePage_SelectGameRootPath);
        if (string.IsNullOrEmpty(gameRootPath))
        {
            return;
        }

        GameRootFolderPath = gameRootPath;
    }

    [RelayCommand]
    private async Task SelectModRootFolder()
    {
        var modRootPath = await SelectFolderInteraction.HandleAsync(Resource.InitializePage_SelectModRootPath);
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
            var messageBox = App.Services.GetRequiredService<MessageBoxService>();
            await messageBox.WarnAsync(Resource.UIErrorMessage_MissingRequiredInfoTip);
            return;
        }
        var settings = App.Services.GetRequiredService<AppSettingService>();
        settings.GameRootFolderPath = GameRootFolderPath;
        settings.ModRootFolderPath = ModRootFolderPath;
        Log.Info("资源目录设置成功");

        WeakReferenceMessenger.Default.Send(new CompleteAppInitializeMessage());
    }

    public Type AppThemes { get; } = typeof(ThemeMode);
}