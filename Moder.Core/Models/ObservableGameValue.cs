using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Moder.Core.Models.Vo;

namespace Moder.Core.Models;

public abstract partial class ObservableGameValue(string key, NodeVo? parent) : ObservableObject
{
    public string Key { get; } = key;
    public bool IsChanged { get; private set; }
    public GameValueType Type { get; protected init; }
    public string TypeString => Type.ToString();
    public NodeVo? Parent { get; } = parent;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        IsChanged = true;
        base.OnPropertyChanged(e);
    }

    [RelayCommand]
    private void RemoveSelfInParent()
    {
        Debug.Assert(Parent != null, "Parent cannot be null");
        if (Parent is null)
        {
	        return;
        }

        Parent.Remove(this);
        if (Parent.Children.Count == 0 && Parent.Parent is not null)
        {
            Parent.Parent.Remove(Parent);
        }
    }
}
