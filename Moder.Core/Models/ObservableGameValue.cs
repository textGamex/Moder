using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Moder.Core.Models.Vo;

namespace Moder.Core.Models;

public abstract class ObservableGameValue(string key, NodeVo? parent) : ObservableObject
{
    public string Key { get; } = key;
    public bool IsChanged { get; private set; }
    public GameValueType Type { get; protected init; }
    public string TypeString => Type.ToString();
    public NodeVo? Parent { get; } = parent;

    public IRelayCommand RemoveSelfInParentCommand => _removeSelfInParentCommand ??= new RelayCommand(RemoveSelfInParent);
    private RelayCommand? _removeSelfInParentCommand;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        IsChanged = true;
        base.OnPropertyChanged(e);
    }

    private void RemoveSelfInParent()
    {
        Debug.Assert(Parent != null, "Parent cannot be null");
        if (Parent is null)
        {
            return;
        }

        Parent.Remove(this);
        // 如果父节点下没有其他子节点，则删除父节点
        if (Parent.Children.Count == 0 && Parent.Parent is not null)
        {
            Parent.Parent.Remove(Parent);
        }
    }
}
