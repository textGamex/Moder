using MemoryPack;
using Windows.Storage;
using Microsoft.Extensions.Logging;

namespace Moder.Core.Services.Config;

[MemoryPackable]
public sealed partial class GlobalSettingService
{
    [MemoryPackOrder(0)]
    public string ModRootFolderPath { get; set; } = string.Empty;

    // TODO: 添加选择按钮
    [MemoryPackOrder(1)]
    public string GameRootFolderPath { get; set; } = string.Empty;

    private const string ConfigFileName = "globalSettings.bin";

    public async Task SaveAsync()
    {
        var storageFolder = ApplicationData.Current.LocalFolder;
        var file = await storageFolder.CreateFileAsync(ConfigFileName, CreationCollisionOption.ReplaceExisting);
        // TODO: System.IO.Pipelines
        await FileIO.WriteBytesAsync(file, MemoryPackSerializer.Serialize(this));
    }

    public static GlobalSettingService Load()
    {
	    var storageFolder = ApplicationData.Current.LocalFolder;
        var filePath = Path.Combine(storageFolder.Path, ConfigFileName);
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
