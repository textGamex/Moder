using MemoryPack;
using Moder.Core.Models;
using NLog;

namespace Moder.Core.Services.Config;

[MemoryPackable]
public sealed partial class AppSettingService
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
    public GameLanguage GameLanguage
    {
        get;
        set => SetProperty(ref field, value);
    } = GameLanguage.Default;

    [MemoryPackOrder(3)]
    public string AppLanguage
    {
        get;
        set => SetProperty(ref field, value);
    } = AppLanguageInfo.Default;

    [MemoryPackIgnore]
    public bool IsChanged { get; private set; }

    [MemoryPackIgnore]
    public bool IsUnchanged => !IsChanged;

    private const string ConfigFileName = "globalSettings.bin";
    private static string ConfigFilePath => Path.Combine(App.AppConfigFolder, ConfigFileName);

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private AppSettingService() { }

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

    public static AppSettingService Load()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return new AppSettingService();
        }

        using var reader = File.OpenRead(ConfigFilePath);
        var array = new Span<byte>(new byte[reader.Length]);
        _ = reader.Read(array);
        var result = MemoryPackSerializer.Deserialize<AppSettingService>(array);

        if (result is null)
        {
            result = new AppSettingService();
        }
        else
        {
            result.IsChanged = false;
        }

        return result;
    }
}
