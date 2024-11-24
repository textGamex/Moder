using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services.Config;
using Moder.Core.Services.GameResources;

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
        ProgressPromptMessage = "加载本地化文件中...";
        var start = Stopwatch.GetTimestamp();
        _ = App.Current.Services.GetRequiredService<LocalisationService>();
        _loadTime = Stopwatch.GetElapsedTime(start);
    }

    private void InitializeCompleteAfter()
    {
        ProgressPromptMessage = $"初始化完成, 耗时: {_loadTime.TotalSeconds:F3} s";
        IsLoading = false;
    }

    [ObservableProperty]
    private string _progressPromptMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;
}
