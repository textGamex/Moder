using Moder.Language.Strings;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Moder.Core.Services;

public sealed class MessageBoxService
{
    public async Task WarnAsync(string message)
    {
        var dialog = MessageBoxManager.GetMessageBoxStandard(
            Resource.Common_Warning,
            message,
            ButtonEnum.Ok,
            Icon.Warning
        );
        await dialog.ShowAsync();
    }

    public async Task ErrorAsync(string message)
    {
        var dialog = MessageBoxManager.GetMessageBoxStandard(
            Resource.Common_Error,
            message,
            ButtonEnum.Ok,
            Icon.Error
        );
        await dialog.ShowAsync();
    }
    
    public async Task InfoAsync(string message)
    {
        var dialog = MessageBoxManager.GetMessageBoxStandard(
            Resource.Common_Tip,
            message,
            ButtonEnum.Ok,
            Icon.Info
        );
        await dialog.ShowAsync();
    }
}
