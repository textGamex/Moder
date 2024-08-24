using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Moder.Core.Models;

public abstract class ObservableGameValue(string key) : ObservableObject
{
    public string Key { get; } = key;
    public bool IsChanged { get; private set; }
    public GameValueType Type { get; protected init; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        IsChanged = true;
        base.OnPropertyChanged(e);
    }
}
