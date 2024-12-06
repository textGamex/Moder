namespace Moder.Core.Models.Game;

public sealed class SpriteInfo(string name, string path)
{
    public string Name { get; } = name;
    /// <summary>
    /// 图片的相对路径
    /// </summary>
    public string Path { get; } = path;
}