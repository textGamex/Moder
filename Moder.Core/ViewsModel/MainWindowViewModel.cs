using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services;
using Moder.Core.Views.Game;

namespace Moder.Core.ViewsModel;

public sealed partial class MainWindowViewModel : ObservableObject
{
    [RelayCommand]
    private void OpenCharacterEditor()
    {
        var tabview = App.Services.GetRequiredService<TabViewNavigationService>();
        tabview.AddSingleTabFromIoc<CharacterEditorControlView>();
    }
}