using Microsoft.UI.Xaml.Media;

namespace Moder.Core.Models;

public enum WindowBackdropType : byte
{
    /// <summary>
    /// 默认值为 <see cref="Mica"/>, 当不支持 <see cref="Mica"/> 时切换到 <see cref="None"/>
    /// </summary>
    Default,

    /// <summary>
    /// <c>null</c>
    /// </summary>
    None,

    /// <summary>
    /// <see cref="DesktopAcrylicBackdrop"/>
    /// </summary>
    Acrylic,

    /// <summary>
    /// <see cref="MicaBackdrop"/>
    /// </summary>
    Mica,

    /// <summary>
    /// <see cref="MicaBackdrop"/>, <see cref="MicaBackdrop.Kind"/> 设置为 <c>BaseAlt</c>
    /// </summary>
    MicaAlt
}
