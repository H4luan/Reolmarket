using System.Windows.Input;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class AddRentalViewModel : ViewModelBase
{
    private int _tenantId;
    private int _shelfId;
    private DateTime _startDate = DateTime.Now;
    private DateTime? _endDate;
    private decimal _monthlyRent;
    private bool _dialogResult;

    public int TenantId
    {
        get => _tenantId;
        set => SetProperty(ref _tenantId, value);
    }

    public int ShelfId
    {
        get => _shelfId;
        set => SetProperty(ref _shelfId, value);
    }

    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    public decimal MonthlyRent
    {
        get => _monthlyRent;
        set => SetProperty(ref _monthlyRent, value);
    }

    public bool DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? RequestClose;

    public AddRentalViewModel()
    {
        OkCommand = new RelayCommand(ExecuteOk, CanExecuteOk);
        CancelCommand = new RelayCommand(ExecuteCancel);
    }

    private bool CanExecuteOk(object? parameter)
    {
        return TenantId > 0 && ShelfId > 0 && MonthlyRent > 0;
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
