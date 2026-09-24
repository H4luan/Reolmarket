using System.Windows.Input;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class AddTenantViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private bool _dialogResult;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public bool DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? RequestClose;

    public AddTenantViewModel()
    {
        OkCommand = new RelayCommand(ExecuteOk, CanExecuteOk);
        CancelCommand = new RelayCommand(ExecuteCancel);
    }

    private bool CanExecuteOk(object? parameter)
    {
        return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email);
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
