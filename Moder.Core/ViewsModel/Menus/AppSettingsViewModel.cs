using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Extensions;
using Moder.Core.Infrastructure;
using Moder.Core.Messages;
using Moder.Core.Models;
using Moder.Core.Models.Game;
using Moder.Core.Services;
using Moder.Core.Services.Config;
using Moder.Language.Strings;
using NLog;

namespace Moder.Core.ViewsModel.Menus;

public sealed partial class AppSettingsViewModel : ObservableValidator
{
    private readonly AppSettingService _settingService;
    private readonly MessageBoxService _messageBox;

    [Required(
        ErrorMessageResourceName = "UIErrorMessage_Required",
        ErrorMessageResourceType = typeof(Resource)
    )]
    [ObservableProperty]
    public partial string GameRootFolderPath { get; set; } = string.Empty;

    [Required(
        ErrorMessageResourceName = "UIErrorMessage_Required",
        ErrorMessageResourceType = typeof(Resource)
    )]
    [ObservableProperty]
    public partial string ModRootFolderPath { get; set; } = string.Empty;

    [ObservableProperty]
    private AppLanguageInfo _selectedAppLanguage;

    [ObservableProperty]
    private GameLanguageInfo _selectedGameLanguage;

    [ObservableProperty]
    private AppThemeInfo _selectedAppTheme;

    public Interaction<string, string> SelectFolderInteraction { get; } = new();

    public AppLanguageInfo[] ApplicationLanguages { get; } =
        [new("跟随系统设置", string.Empty), new("简体中文", "zh-CN"), new("English", "en-US")];

    public AppThemeInfo[] AppThemes { get; } =
        [
            new(Resource.Common_UseSystemSetting, ThemeMode.Default),
            new(Resource.ThemeMode_Light, ThemeMode.Light),
            new(Resource.ThemeMode_Dark, ThemeMode.Dark)
        ];

    public GameLanguageInfo[] GameLanguages { get; } =
        [
            new("跟随系统设置", GameLanguage.Default),
            new("简体中文", GameLanguage.Chinese),
            new("英文", GameLanguage.English),
            new("日语", GameLanguage.Japanese),
            new("俄语", GameLanguage.Russian),
            new("法语", GameLanguage.French),
            new("波兰语", GameLanguage.Polish),
            new("德语", GameLanguage.German),
            new("西班牙语", GameLanguage.Spanish),
            new("巴西葡萄牙语", GameLanguage.Portuguese)
        ];

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public AppSettingsViewModel(AppSettingService settingService, MessageBoxService messageBox)
    {
        _settingService = settingService;
        _messageBox = messageBox;

        InitAppLanguage();
        InitGameLanguage();
        InitAppTheme();
    }

    [MemberNotNull(nameof(_selectedAppTheme))]
    [SuppressMessage(
        "CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
        "MVVMTK0034:Direct field reference to [ObservableProperty] backing field",
        Justification = "需要跳过属性变化通知"
    )]
    private void InitAppTheme()
    {
        var theme = Array.Find(AppThemes, info => info.Mode == _settingService.AppTheme) ?? AppThemes[0];
        _selectedAppTheme = theme;
    }

    [MemberNotNull(nameof(_selectedAppLanguage))]
    [SuppressMessage(
        "CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
        "MVVMTK0034:Direct field reference to [ObservableProperty] backing field",
        Justification = "需要跳过属性变化通知"
    )]
    private void InitAppLanguage()
    {
        var language = Array.Find(ApplicationLanguages, info => info.Code == _settingService.AppLanguageCode);
        if (language is null)
        {
            Log.Warn(
                "未找到匹配的语言代码, Code:{Code}, 存在: [{LanguageArray}]",
                _settingService.AppLanguageCode,
                string.Join(", ", ApplicationLanguages.Select(info => info.Code))
            );
            _selectedAppLanguage = ApplicationLanguages[0];
        }
        else
        {
            _selectedAppLanguage = language;
        }
    }

    [MemberNotNull(nameof(_selectedGameLanguage))]
    [SuppressMessage(
        "CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
        "MVVMTK0034:Direct field reference to [ObservableProperty] backing field",
        Justification = "需要跳过属性变化通知"
    )]
    private void InitGameLanguage()
    {
        var language = Array.Find(GameLanguages, info => info.Type == _settingService.GameLanguage);
        if (language is null)
        {
            Log.Warn(
                "未找到匹配的游戏语言代码, Code:{Code}, 存在: [{LanguageArray}]",
                _settingService.GameLanguage,
                string.Join(", ", GameLanguages.Select(info => info.DisplayName))
            );
            _selectedGameLanguage = GameLanguages[0];
        }
        else
        {
            _selectedGameLanguage = language;
        }
    }

    partial void OnSelectedAppLanguageChanged(AppLanguageInfo value)
    {
        _settingService.AppLanguageCode = value.Code;
        _ = _messageBox.InfoAsync(Resource.SettingsPage_MustRestart);
    }

    partial void OnSelectedGameLanguageChanged(GameLanguageInfo value)
    {
        _settingService.GameLanguage = value.Type;
        _ = _messageBox.InfoAsync(Resource.SettingsPage_MustRestart);
    }
    
    partial void OnSelectedAppThemeChanged(AppThemeInfo value)
    {
        App.Current.RequestedThemeVariant = value.Mode.ToThemeVariant();
        _settingService.AppTheme = value.Mode;
    }

    [RelayCommand]
    private async Task SelectGameRootFolder()
    {
        var gameRootPath = await SelectFolderInteraction.HandleAsync(
            Resource.InitializePage_SelectGameRootPath
        );
        if (string.IsNullOrEmpty(gameRootPath))
        {
            return;
        }

        GameRootFolderPath = gameRootPath;
    }

    [RelayCommand]
    private async Task SelectModRootFolder()
    {
        var modRootPath = await SelectFolderInteraction.HandleAsync(
            Resource.InitializePage_SelectModRootPath
        );
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

        WeakReferenceMessenger.Default.Send(new CompleteAppSettingsMessage());
    }
}
