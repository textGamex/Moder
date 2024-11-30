using Microsoft.UI.Xaml.Controls;
using Moder.Language.Strings;

namespace Moder.Core.Services;

public sealed class MessageBoxService
{
    public async Task WarnAsync(string message)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = App.Current.XamlRoot,
            Title = Resource.Common_Warning,
            Content = message,
            CloseButtonText = Resource.Common_Ok
        };
        await dialog.ShowAsync();
    }

    public async Task ErrorAsync(string message)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = App.Current.XamlRoot,
            Title = Resource.Common_Error,
            Content = message,
            CloseButtonText = Resource.Common_Ok
        };
        await dialog.ShowAsync();
    }
}