using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services.Config;
using Moder.Core.Services.GameResources;
using Moder.Language.Strings;

namespace Moder.Core.ViewsModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private TimeSpan _loadTime;

    public MainWindowViewModel(GlobalSettingService globalSettingService)
    {
        if (string.IsNullOrEmpty(globalSettingService.GameRootFolderPath))
        {
            return;
        }

        _isLoading = true;
        _ = Task.Run(InitializeResources)
            .ContinueWith(_ => App.Current.DispatcherQueue.TryEnqueue(InitializeCompleteAfter));
    }

    private void InitializeResources()
    {
        ProgressPromptMessage = Resource.Menu_LoadingTip;
        var start = Stopwatch.GetTimestamp();
        _ = App.Current.Services.GetRequiredService<LocalisationService>();
        App.Current.Services.GetRequiredService<SpriteService>();
        _loadTime = Stopwatch.GetElapsedTime(start);
    }

    private void InitializeCompleteAfter()
    {
        ProgressPromptMessage = string.Format(Resource.Menu_LoadingCompletedTip, _loadTime.TotalSeconds);
        IsLoading = false;
    }

    [ObservableProperty]
    private string _progressPromptMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;
}
