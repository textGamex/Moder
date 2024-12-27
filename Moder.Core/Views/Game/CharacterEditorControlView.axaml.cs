using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.TextMate;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Editor;
using Moder.Core.Infrastructure;
using Moder.Core.ViewsModel.Game;
using Moder.Language.Strings;

namespace Moder.Core.Views.Game;

public sealed partial class CharacterEditorControlView : UserControl, ITabViewItem, IClosed
{
    public string Header => Resource.Menu_CharacterEditor;
    public string Id => nameof(CharacterEditorControlView);
    public string ToolTip => Header;

    private CharacterEditorControlViewModel ViewModel { get; }

    public CharacterEditorControlView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<CharacterEditorControlViewModel>();
        DataContext = ViewModel;

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
