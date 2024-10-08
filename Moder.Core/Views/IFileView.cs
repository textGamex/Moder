namespace Moder.Core.Views;

/// <summary>
/// 可视化文件接口
/// </summary>
public interface IFileView
{
	public string Title { get; }
	public string FullPath { get; }
}