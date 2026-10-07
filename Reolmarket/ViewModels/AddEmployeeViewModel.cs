using System.Windows.Input;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class AddEmployeeViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private bool _dialogResult;

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string Phone
    {
        get => _phone;
        set
        {
            if (SetProperty(ref _phone, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? RequestClose;

    public AddEmployeeViewModel()
    {
        OkCommand = new RelayCommand(ExecuteOk, CanExecuteOk);
        CancelCommand = new RelayCommand(ExecuteCancel);
    }

    private bool CanExecuteOk(object? parameter)
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(Phone);
    }

    private void ExecuteOk(object? parameter)
    {
        DialogResult = true;
        RequestClose?.Invoke();
    }

    private void ExecuteCancel(object? parameter)
    {
        DialogResult = false;
        RequestClose?.Invoke();
    }
}