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
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    [MemoryPackOrder(1)]
    public string GameRootFolderPath
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    [MemoryPackOrder(2)]
    public ElementTheme AppThemeMode
    {
        get;
        set => SetProperty(ref field, value);
    } = ElementTheme.Default;

    [MemoryPackOrder(3)]
    public GameLanguage GameLanguage
    {
        get;
        set => SetProperty(ref field, value);
    } = GameLanguage.Default;

    [MemoryPackOrder(4)]
    public WindowBackdropType WindowBackdropType
    {
        get;
        set => SetProperty(ref field, value);
    } = WindowBackdropType.Default;

    [MemoryPackOrder(5)]
    public string AppLanguage
    {
        get;
        set => SetProperty(ref field, value);
    } = LanguageInfo.Default;

    [MemoryPackIgnore]
    public bool IsChanged { get; private set; }

    [MemoryPackIgnore]
    public bool IsUnchanged => !IsChanged;

    private const string ConfigFileName = "globalSettings.bin";
    private static string ConfigFilePath => Path.Combine(App.ConfigFolder, ConfigFileName);

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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
