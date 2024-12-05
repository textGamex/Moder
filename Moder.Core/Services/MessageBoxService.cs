using Moder.Language.Strings;
using MsBox.Avalonia;

namespace Moder.Core.Services;

public sealed class MessageBoxService
{
    public async Task WarnAsync(string message)
    {
        var dialog = MessageBoxManager.GetMessageBoxStandard(Resource.Common_Warning, message);
        await dialog.ShowAsync();
    }

    public async Task ErrorAsync(string message)
    {
        var dialog = MessageBoxManager.GetMessageBoxStandard(Resource.Common_Error, message);
        await dialog.ShowAsync();
    }
}
