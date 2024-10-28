using Microsoft.UI.Xaml.Controls;
using Moder.Core.Models.Character;
using Moder.Core.Services.GameResources;
using Moder.Core.ViewsModels.Game;

namespace Moder.Core.Views.Game;

public sealed partial class CharacterEditorControlView : UserControl
{
    public CharacterEditorControlViewModel ViewModel { get; }

    public CharacterEditorControlView(CharacterEditorControlViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();
    }
}
