using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Moder.Core.Models;
using Moder.Language.Strings;

namespace Moder.Core.ViewsModel.Menus;

public sealed class RenameFileControlViewModel : ObservableValidator
{
    public bool IsValid => IsValidValue();

    private string _errorMessage = Resource.RenameFile_InvalidFileOrFolderName;

    [CustomValidation(typeof(RenameFileControlViewModel), nameof(ValidateName))]
    public string NewName
    {
        get;
        set
        {
            SetProperty(ref field, value, true);
            _dialog.IsPrimaryButtonEnabled = IsValid;
        }
    }

    public int SelectionEnd { get; }

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
            SelectionEnd = length == -1 ? 0 : length;
        }
    }

    public static ValidationResult? ValidateName(string name, ValidationContext context)
    {
        var instance = (RenameFileControlViewModel)context.ObjectInstance;

        if (instance.IsValid)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(instance._errorMessage);
    }

    private bool IsValidValue()
    {
        var isValid = !string.IsNullOrWhiteSpace(NewName);
        _errorMessage = Resource.RenameFile_InvalidFileOrFolderName;

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
            _errorMessage = Resource.RenameFile_NameContainInvalidChar;
        }
        if (isValid && HasEqualsNameItem())
        {
            isValid = false;
            _errorMessage = string.Format(Resource.RenameFile_NameAlreadyExists, NewName);
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
}
