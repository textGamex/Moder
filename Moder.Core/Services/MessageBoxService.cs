using Microsoft.UI.Xaml.Controls;

namespace Moder.Core.Services;

public sealed class MessageBoxService
{
    public async Task WarnAsync(string message)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = App.Current.MainWindow.Content.XamlRoot,
            Title = "警告",
            Content = message,
            CloseButtonText = "确定"
        };
        await dialog.ShowAsync();
    }

    public async Task ErrorAsync(string message)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = App.Current.MainWindow.Content.XamlRoot,
            Title = "错误",
            Content = message,
            CloseButtonText = "确定"
        };
        await dialog.ShowAsync();
    }
}