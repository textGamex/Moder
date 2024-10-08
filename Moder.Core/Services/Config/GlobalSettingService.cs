using MemoryPack;
using Microsoft.UI.Xaml;

namespace Moder.Core.Services.Config;

[MemoryPackable]
public sealed partial class GlobalSettingService
{
    [MemoryPackOrder(0)]
    public string ModRootFolderPath { get; set; } = string.Empty;

    [MemoryPackOrder(1)]
    public string GameRootFolderPath { get; set; } = string.Empty;

    [MemoryPackOrder(2)]
    public ElementTheme AppThemeMode { get; set; } = ElementTheme.Default;

    private const string ConfigFileName = "globalSettings.bin";
    private static string ConfigFilePath => Path.Combine(App.ConfigFolder, ConfigFileName);


    public void Save()
    {
        // TODO: System.IO.Pipelines
        File.WriteAllBytes(ConfigFilePath, MemoryPackSerializer.Serialize(this));
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
        return result ?? new GlobalSettingService();
    }
}
