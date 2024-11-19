using MemoryPack;
using Microsoft.UI.Xaml;
using Moder.Core.Models;
using NLog;

namespace Moder.Core.Services.Config;

[MemoryPackable]
public sealed partial class GlobalSettingService
{
    [MemoryPackOrder(0)]
    public string ModRootFolderPath
    {
        get => _modRootFolderPath;
        set => SetProperty(ref _modRootFolderPath, value);
    }
    private string _modRootFolderPath = string.Empty;

    [MemoryPackOrder(1)]
    public string GameRootFolderPath
    {
        get => _gameRootFolderPath;
        set => SetProperty(ref _gameRootFolderPath, value);
    }
    private string _gameRootFolderPath = string.Empty;

    [MemoryPackOrder(2)]
    public ElementTheme AppThemeMode
    {
        get => _appThemeMode;
        set => SetProperty(ref _appThemeMode, value);
    }
    private ElementTheme _appThemeMode = ElementTheme.Default;

    [MemoryPackOrder(3)]
    public GameLanguage GameLanguage
    {
        get => _gameLanguage;
        set => SetProperty(ref _gameLanguage, value);
    }
    private GameLanguage _gameLanguage = GameLanguage.Default;
    
    [MemoryPackOrder(4)]
    public WindowBackdropType WindowBackdropType
    {
        get => _windowBackdropType;
        set => SetProperty(ref _windowBackdropType, value);
    }
    private WindowBackdropType _windowBackdropType = WindowBackdropType.Default;

    [MemoryPackIgnore]
    public bool IsChanged { get; private set; }
    [MemoryPackIgnore]
    public bool IsUnchanged => !IsChanged;

    private const string ConfigFileName = "globalSettings.bin";
    private static string ConfigFilePath => Path.Combine(App.ConfigFolder, ConfigFileName);

    private static readonly Logger Log =
        LogManager.GetCurrentClassLogger();

    private GlobalSettingService() { }

    private void SetProperty<T>(ref T field, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return;
        }
        field = newValue;
        IsChanged = true;
    }

    /// <summary>
    /// 如果有更改，保存更改
    /// </summary>
    public void SaveChanged()
    {
        if (IsUnchanged)
        {
            Log.Info("配置文件未改变, 跳过写入");
            return;
        }
        Log.Info("配置文件保存中...");
        // TODO: System.IO.Pipelines
        File.WriteAllBytes(ConfigFilePath, MemoryPackSerializer.Serialize(this));
        IsChanged = false;
        Log.Info("配置文件保存完成");
    }

    public static GlobalSettingService Load()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return new GlobalSettingService();
        }

        using var reader = File.OpenRead(ConfigFilePath);
        var array = new Span<byte>(new byte[reader.Length]);
        _ = reader.Read(array);
        var result = MemoryPackSerializer.Deserialize<GlobalSettingService>(array);

        if (result is null)
        {
            result = new GlobalSettingService();
        }
        else
        {
            result.IsChanged = false;
        }

        return result;
    }
}
