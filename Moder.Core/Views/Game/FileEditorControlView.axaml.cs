using System.Diagnostics;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Moder.Core.Infrastructure;
using Moder.Core.Models;

namespace Moder.Core.Views.Game;

public sealed partial class FileEditorControlView : UserControl, ITabViewItem
{
    public string Header => _fileItem.Name;
    public string Id => _fileItem.FullPath;
    public string ToolTip => _fileItem.FullPath;
    public IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Library };

    private readonly SystemFileItem _fileItem;

    public FileEditorControlView(SystemFileItem fileItem)
    {
        Debug.Assert(fileItem.IsFile);
        _fileItem = fileItem;
        InitializeComponent();

        Editor.Text = File.ReadAllText(fileItem.FullPath, Encodings.Utf8NotBom);
    }
}
