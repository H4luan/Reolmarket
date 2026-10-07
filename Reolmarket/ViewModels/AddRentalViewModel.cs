using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class AddRentalViewModel : ViewModelBase
{
    private readonly List<Rental> _existingRentals;

    private int _tenantId;
    private int _shelfId;
    private DateTime _startDate = DateTime.Today;
    private DateTime? _endDate;
    private decimal _monthlyRent;
    private bool _dialogResult;

    public ObservableCollection<Tenant> Tenants { get; }
    public ObservableCollection<Shelf> Shelves { get; }

    public int TenantId
    {
        get => _tenantId;
        set
        {
            if (SetProperty(ref _tenantId, value))
            {
                UpdateMonthlyRent();
            }
        }
    }

    public int ShelfId
    {
        get => _shelfId;
        set => SetProperty(ref _shelfId, value);
    }

    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            if (SetProperty(ref _startDate, value))
            {
                UpdateMonthlyRent();
            }
        }
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    public decimal MonthlyRent
    {
        get => _monthlyRent;
        private set
        {
            if (SetProperty(ref _monthlyRent, value))
            {
                OnPropertyChanged(nameof(MonthlyRentText));
            }
        }
    }

    public string MonthlyRentText =>
        TenantId == 0
            ? "Vælg en lejer for at se prisen."
            : $"{MonthlyRent:N0} kr. pr. måned pr. reol";

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
        IEnumerable<Shelf> shelves,
        IEnumerable<Rental> existingRentals)
    {
        Tenants = new ObservableCollection<Tenant>(tenants);
        Shelves = new ObservableCollection<Shelf>(shelves);
        _existingRentals = existingRentals.ToList();

        OkCommand = new RelayCommand(ExecuteOk, CanExecuteOk);
        CancelCommand = new RelayCommand(ExecuteCancel);
    }

    private void UpdateMonthlyRent()
    {
        if (TenantId <= 0)
        {
            MonthlyRent = 0;
            return;
        }

        int existingActiveShelves = _existingRentals.Count(rental =>
            rental.TenantId == TenantId &&
            rental.IsActive(StartDate.Date));

        int totalActiveShelvesIncludingNew = existingActiveShelves + 1;

        MonthlyRent = totalActiveShelvesIncludingNew switch
        {
            1 => 850m,
            <= 3 => 825m,
            _ => 800m
        };
    }

    private bool CanExecuteOk(object? parameter)
    {
        return TenantId > 0 &&
               ShelfId > 0 &&
               MonthlyRent > 0 &&
               (!EndDate.HasValue ||
                EndDate.Value.Date >= StartDate.Date);
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