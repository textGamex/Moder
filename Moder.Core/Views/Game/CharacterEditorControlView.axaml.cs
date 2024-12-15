﻿using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Infrastructure;
using Moder.Core.ViewsModel.Game;
using Moder.Language.Strings;

namespace Moder.Core.Views.Game;

public sealed partial class CharacterEditorControlView : UserControl, ITabViewItem
{
    public string Header => Resource.Menu_CharacterEditor;
    public string Id => nameof(CharacterEditorControlView);
    public string ToolTip => Header;

    public CharacterEditorControlView()
    {
        InitializeComponent();
        var viewModel = App.Services.GetRequiredService<CharacterEditorControlViewModel>();
        DataContext = viewModel;
    }
}