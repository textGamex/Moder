using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Infrastructure;
using Moder.Core.ViewsModel.Game;
using Moder.Language.Strings;

namespace Moder.Core.Views.Game;

public sealed partial class CharacterEditorControlView : UserControl, ITabViewItem, IClosed
{
    public string Header => Resource.Menu_CharacterEditor;
    public string Id => nameof(CharacterEditorControlView);
    public string ToolTip => Header;
    public IconSource Icon { get; } = new SymbolIconSource { Symbol = Symbol.Edit };

    private CharacterEditorControlViewModel ViewModel { get; }

    public CharacterEditorControlView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<CharacterEditorControlViewModel>();
        DataContext = ViewModel;

        Editor.SetGrammar(".txt");
        ViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.GeneratedText))
            {
                Editor.Text = ViewModel.GeneratedText;
            }
        };
    }

    public void Close()
    {
        ViewModel.Close();
    }
}
