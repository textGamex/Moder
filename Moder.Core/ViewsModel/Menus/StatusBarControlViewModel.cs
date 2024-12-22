using System.Diagnostics;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services.GameResources.Localization;
using Moder.Language.Strings;

namespace Moder.Core.ViewsModel.Menus;

public sealed partial class StatusBarControlViewModel : ObservableObject
{
    private TimeSpan _loadTime;

    [ObservableProperty]
    public partial string ProgressPromptMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public StatusBarControlViewModel()
    {
        IsLoading = true;
        _ = Task.Run(InitializeResources)
            .ContinueWith(_ => Dispatcher.UIThread.Post(InitializeCompleteAfter));
    }

    private void InitializeResources()
    {
        ProgressPromptMessage = Resource.Menu_LoadingTip;
        var start = Stopwatch.GetTimestamp();
        _ = App.Services.GetRequiredService<LocalizationService>();
        _loadTime = Stopwatch.GetElapsedTime(start);
    }

    private void InitializeCompleteAfter()
    {
        ProgressPromptMessage = string.Format(Resource.Menu_LoadingCompletedTip, _loadTime.TotalSeconds);
        IsLoading = false;
    }
}
