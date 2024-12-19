using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace Moder.Core.Controls;

public sealed class DirectorySelector : TemplatedControl
{
    public string SelectorCaption
    {
        get => GetValue(SelectorCaptionProperty);
        set => SetValue(SelectorCaptionProperty, value);
    }
    public static readonly StyledProperty<string> SelectorCaptionProperty
    = AvaloniaProperty.Register<DirectorySelector, string>(nameof(SelectorCaption));
    
    public string DirectoryPath
    {
        get => GetValue(DirectoryPathProperty);
        set => SetValue(DirectoryPathProperty, value);
    }
    public static readonly StyledProperty<string> DirectoryPathProperty = AvaloniaProperty.Register<
        DirectorySelector,
        string
    >(nameof(DirectoryPath), enableDataValidation:true, defaultBindingMode:BindingMode.TwoWay);

    public ICommand SelectDirectoryCommand
    {
        get => GetValue(SelectDirectoryCommandProperty);
        set => SetValue(SelectDirectoryCommandProperty, value);
    }
    public static readonly StyledProperty<ICommand> SelectDirectoryCommandProperty =
        AvaloniaProperty.Register<DirectorySelector, ICommand>(nameof(SelectDirectoryCommand));
    
    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        if (property == DirectoryPathProperty)
        {
            DataValidationErrors.SetError(this, error);
        }
    }
}
