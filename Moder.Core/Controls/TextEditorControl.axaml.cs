using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.TextMate;
using Moder.Core.Editor;

namespace Moder.Core.Controls;

public sealed partial class TextEditorControl : UserControl
{
    public string Text
    {
        get => TextEditor.Text;
        set => TextEditor.Text = value;
    }
    private TextMate.Installation _installation;
    private ParadoxRegistryOptions _options;

    public TextEditorControl()
    {
        InitializeComponent();
        InitializeTextEditor();

        ActualThemeVariantChanged += (_, _) =>
        {
            _installation.SetTheme(_options.LoadTheme(ActualThemeVariant));
        };
    }

    // TODO: 状态栏
    [MemberNotNull(nameof(_installation))]
    [MemberNotNull(nameof(_options))]
    private void InitializeTextEditor()
    {
        _options = new ParadoxRegistryOptions(App.Current.ActualThemeVariant);
        TextEditor.Options.HighlightCurrentLine = true;
        TextEditor.Options.EnableTextDragDrop = true;
        TextEditor.TextArea.RightClickMovesCaret = true;
        TextEditor.Options.ShowBoxForControlCharacters = true;
        TextEditor.Options.AllowToggleOverstrikeMode = true;
        TextEditor.ShowLineNumbers = true;
        _installation = TextEditor.InstallTextMate(_options);

        _installation.SetGrammar("source.hoi4");
        _installation.AppliedTheme += ChangeThemeOnAppliedTheme;

        ApplyThemeColorsToEditor(_installation);
    }

    private void ChangeThemeOnAppliedTheme(object? sender, TextMate.Installation e)
    {
        ApplyThemeColorsToEditor(e);
    }

    private void ApplyThemeColorsToEditor(TextMate.Installation installation)
    {
        ApplyBrushAction(installation, "editor.background", brush => TextEditor.Background = brush);
        ApplyBrushAction(installation, "editor.foreground", brush => TextEditor.Foreground = brush);

        if (
            !ApplyBrushAction(
                installation,
                "editor.selectionBackground",
                brush => TextEditor.TextArea.SelectionBrush = brush
            )
        )
        {
            if (App.Current.TryGetResource("TextAreaSelectionBrush", out var resourceObject))
            {
                if (resourceObject is IBrush brush)
                {
                    TextEditor.TextArea.SelectionBrush = brush;
                }
            }
        }

        if (
            !ApplyBrushAction(
                installation,
                "editor.lineHighlightBackground",
                brush =>
                {
                    TextEditor.TextArea.TextView.CurrentLineBackground = brush;
                    TextEditor.TextArea.TextView.CurrentLineBorder = new Pen(brush);
                }
            )
        )
        {
            TextEditor.TextArea.TextView.SetDefaultHighlightLineColors();
        }

        if (
            !ApplyBrushAction(
                installation,
                "editorLineNumber.foreground",
                brush => TextEditor.LineNumbersForeground = brush
            )
        )
        {
            TextEditor.LineNumbersForeground = TextEditor.Foreground;
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
}
