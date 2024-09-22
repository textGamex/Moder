using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Controls;

public sealed partial class BaseLeaf : Control
{
    public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
        nameof(Key),
        typeof(string),
        typeof(BaseLeaf),
        new PropertyMetadata(null)
    );

    public static readonly DependencyProperty SlotContentProperty = DependencyProperty.Register(
        nameof(SlotContent),
        typeof(object),
        typeof(BaseLeaf),
        new PropertyMetadata(null)
    );

    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
        nameof(Type),
        typeof(string),
        typeof(BaseLeaf),
        new PropertyMetadata(null)
    );

    public string? Type
    {
        get => (string?)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public object? SlotContent
    {
        get => GetValue(SlotContentProperty);
        set => SetValue(SlotContentProperty, value);
    }

    public string? Key
    {
        get => (string?)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public BaseLeaf()
    {
        DefaultStyleKey = typeof(BaseLeaf);
    }
}
