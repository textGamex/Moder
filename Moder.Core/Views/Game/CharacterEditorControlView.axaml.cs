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

        InitializeTextEditor();
        ViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.GeneratedText))
            {
                Editor.Text = ViewModel.GeneratedText;
            }
        };
    }

    // TODO: 状态栏
    private void InitializeTextEditor()
    {
        var options = new ParadoxRegistryOptions(App.Current.ActualThemeVariant);
        Editor.Options.HighlightCurrentLine = true;
        var installation = Editor.InstallTextMate(options);

        installation.SetGrammar("source.hoi4");

        ApplyThemeColorsToEditor(installation);
    }

    private void ApplyThemeColorsToEditor(TextMate.Installation installation)
    {
        ApplyBrushAction(installation, "editor.background", brush => Editor.Background = brush);
        ApplyBrushAction(installation, "editor.foreground", brush => Editor.Foreground = brush);

        if (
            !ApplyBrushAction(
                installation,
                "editor.selectionBackground",
                brush => Editor.TextArea.SelectionBrush = brush
            )
        )
        {
            if (App.Current.TryGetResource("TextAreaSelectionBrush", out var resourceObject))
            {
                if (resourceObject is IBrush brush)
                {
                    Editor.TextArea.SelectionBrush = brush;
                }
            }
        }

        if (
            !ApplyBrushAction(
                installation,
                "editor.lineHighlightBackground",
                brush =>
                {
                    Editor.TextArea.TextView.CurrentLineBackground = brush;
                    Editor.TextArea.TextView.CurrentLineBorder = new Pen(brush);
                }
            )
        )
        {
            Editor.TextArea.TextView.SetDefaultHighlightLineColors();
        }

        if (
            !ApplyBrushAction(
                installation,
                "editorLineNumber.foreground",
                brush => Editor.LineNumbersForeground = brush
            )
        )
        {
            Editor.LineNumbersForeground = Editor.Foreground;
        }
    }

    private static bool ApplyBrushAction(
        TextMate.Installation e,
        string colorKeyNameFromJson,
        Action<IBrush> applyColorAction
    )
    {
        if (!e.TryGetThemeColor(colorKeyNameFromJson, out var colorString))
        {
            return false;
        }

        if (!Color.TryParse(colorString, out var color))
        {
            return false;
        }

        var colorBrush = new SolidColorBrush(color);
        applyColorAction(colorBrush);
        return true;
    }

    private void TextMateInstallationOnAppliedTheme(object sender, TextMate.Installation e)
    {
        ApplyThemeColorsToEditor(e);
    }

    public void Close()
    {
        ViewModel.Close();
    }
}
