using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using Reolmarket.Domain;
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

    public ObservableCollection<Tenant> Tenants { get; }
    public ObservableCollection<Shelf> Shelves { get; }

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

    public AddRentalViewModel(
        IEnumerable<Tenant> tenants,
        IEnumerable<Shelf> shelves)
    {
        Tenants = new ObservableCollection<Tenant>(tenants);
        Shelves = new ObservableCollection<Shelf>(shelves);

        OkCommand = new RelayCommand(ExecuteOk, CanExecuteOk);
        CancelCommand = new RelayCommand(ExecuteCancel);
    }

    private bool CanExecuteOk(object? parameter)
    {
        return TenantId > 0 &&
               ShelfId > 0 &&
               MonthlyRent > 0;
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