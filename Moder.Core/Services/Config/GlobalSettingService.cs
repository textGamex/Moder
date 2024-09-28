using MemoryPack;

namespace Moder.Core.Services.Config;

[MemoryPackable]
public sealed partial class GlobalSettingService
{
    [MemoryPackOrder(0)]
    public string ModRootFolderPath { get; set; } = string.Empty;

    [MemoryPackOrder(1)]
    public string GameRootFolderPath { get; set; } = string.Empty;

    private const string ConfigFileName = "globalSettings.bin";

    public void Save()
    {
        var filePath = Path.Combine(App.ConfigFolder, ConfigFileName);
        // TODO: System.IO.Pipelines
        File.WriteAllBytes(filePath, MemoryPackSerializer.Serialize(this));
    }

    public static GlobalSettingService Load()
    {
        var filePath = Path.Combine(App.ConfigFolder, ConfigFileName);
        if (!File.Exists(filePath))
        {
            return new GlobalSettingService();
        }

        using var reader = File.OpenRead(filePath);
        var array = new Span<byte>(new byte[reader.Length]);
        _ = reader.Read(array);
        var result = MemoryPackSerializer.Deserialize<GlobalSettingService>(array);
        return result ?? new GlobalSettingService();
    }
}
