namespace Moder.Core.Models;

public enum WindowBackdropType : byte
{
    /// <summary>
    /// 默认值为 Mica, 当不支持 Mica 时切换到 None
    /// </summary>
    Default,
    None,
    Acrylic,
    Mica,
    MicaAlt
}
