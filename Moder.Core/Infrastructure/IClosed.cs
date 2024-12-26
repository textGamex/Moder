namespace Moder.Core.Infrastructure;

/// <summary>
/// 与 <see cref="IDisposable"/> 的作用一样, 但为了能使用 Ioc 容器管理 Transient 注入的 <see cref="ITabViewItem"/>,
/// 所以绕开 <see cref="IDisposable"/> 单独定义一个接口.
/// <p/>
/// 注意: 只有在 TabView 中使用的类才应该使用此接口来释放资源, 其他类仍应该使用 <see cref="IDisposable"/>
/// </summary>
public interface IClosed
{
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Close();
}
