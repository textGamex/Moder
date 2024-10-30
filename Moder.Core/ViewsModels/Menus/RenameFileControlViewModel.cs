using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace Moder.Core.ViewsModels.Menus;

public sealed partial class RenameFileControlViewModel : ObservableObject
{
    public bool IsValid => IsValidValue();

    public int SelectionLength { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = "无效的文件或文件夹名称";

    private readonly ContentDialog _dialog;
    private readonly SystemFileItem _fileItem;
    private readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
    private readonly char[] _invalidChars = Path.GetInvalidPathChars();

    public RenameFileControlViewModel(ContentDialog dialog, SystemFileItem fileItem)
    {
        _dialog = dialog;
        _fileItem = fileItem;
        NewName = fileItem.Name;

        if (fileItem.IsFile)
        {
            var length = fileItem.Name.IndexOf('.');
            SelectionLength = length == -1 ? 0 : length;
        }
    }

    private bool IsValidValue()
    {
        var isValid = !string.IsNullOrWhiteSpace(NewName);
        ErrorMessage = "无效的文件或文件夹名称";
        if (isValid && NewName.Length != 0)
        {
            isValid = NewName[0] != ' ' && NewName[^1] != ' ';
        }
        if (isValid)
        {
            if (_fileItem.IsFile)
            {
                isValid = NewName.IndexOfAny(_invalidFileNameChars) == -1;
            }
            else
            {
                isValid = NewName.IndexOfAny(_invalidChars) == -1;
            }
            ErrorMessage = "文件或文件夹名称包含非法字符";
        }
        if (isValid && HasEqualsNameItem())
        {
            isValid = false;
            ErrorMessage = $"此位置已存在同名文件或文件夹 {NewName}, 请重新命名";
        }
        return isValid;
    }

    private bool HasEqualsNameItem()
    {
        if (_fileItem.Parent is null)
        {
            return false;
        }

        return _fileItem.Parent.Children.Any(item =>
            !ReferenceEquals(item, _fileItem) && item.Name == NewName
        );
    }

    partial void OnNewNameChanged(string value)
    {
        _dialog.IsPrimaryButtonEnabled = IsValid;
    }
}
