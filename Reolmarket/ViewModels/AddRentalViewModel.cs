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
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;
    private bool _dialogResult;
    private Shelf? _selectedNeighborShelf;
    private string _neighborMessage =
        "Vælg en lejer for at se mulige naboreoler.";

    public ObservableCollection<Tenant> Tenants { get; }
    public ObservableCollection<Shelf> Shelves { get; }
    public ObservableCollection<Shelf> NeighborShelves { get; } = new();
    public ObservableCollection<PaymentMethod> PaymentMethods { get; } = new()
    { PaymentMethod.Cash, PaymentMethod.MobilePay };

    public PaymentMethod SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set => SetProperty(ref _selectedPaymentMethod, value);
    }


    public Shelf? SelectedNeighborShelf
    {
        get => _selectedNeighborShelf;
        set
        {
            if (SetProperty(ref _selectedNeighborShelf, value) &&
                value is not null)
            {
                ShelfId = value.ShelfId;
            }
        }
    }

    public string NeighborMessage
    {
        get => _neighborMessage;
        set => SetProperty(ref _neighborMessage, value);
    }

    

    public int ShelfId
    {
        get => _shelfId;
        set => SetProperty(ref _shelfId, value);
    }

    public int TenantId
    {
        get => _tenantId;
        set
        {
            if (SetProperty(ref _tenantId, value))
            {
                UpdateMonthlyRent();
                UpdateNeighborShelves();
            }
        }
    }

    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            if (SetProperty(ref _startDate, value))
            {
                UpdateMonthlyRent();
                UpdateNeighborShelves();
            }
        }
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set
        {
            if (SetProperty(ref _endDate, value))
            {
                UpdateNeighborShelves();
            }
        }
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

    private void UpdateNeighborShelves()
    {
        NeighborShelves.Clear();
        SelectedNeighborShelf = null;

        if (TenantId <= 0)
        {
            NeighborMessage = "Vælg en lejer for at se mulige naboreoler.";
            return;
        }

        List<int> tenantShelfNumbers = _existingRentals
            .Where(rental =>
                rental.TenantId == TenantId &&
                rental.IsActive(StartDate.Date))
            .Select(rental =>
                Shelves.FirstOrDefault(
                    shelf => shelf.ShelfId == rental.ShelfId)?.ShelfNumber)
            .Where(number => number.HasValue)
            .Select(number => number.GetValueOrDefault())
            .ToList();

        if (tenantShelfNumbers.Count == 0)
        {
            NeighborMessage =
                "Lejeren har ingen aktiv reol på den valgte startdato.";
            return;
        }

        DateTime requestedEndDate =
            EndDate?.Date ?? DateTime.MaxValue.Date;

        List<Shelf> availableNeighbors = Shelves
            .Where(shelf =>
                tenantShelfNumbers.Any(existingNumber =>
                    ShelfLayoutMap.AreAdjacent(
                        shelf.ShelfNumber,
                        existingNumber)))
            .Where(shelf => !_existingRentals.Any(rental =>
                rental.ShelfId == shelf.ShelfId &&
                rental.StartDate.Date <= requestedEndDate &&
                (rental.EndDate is null ||
                 rental.EndDate.Value.Date >= StartDate.Date)))
            .OrderBy(shelf => shelf.ShelfNumber)
            .ToList();

        foreach (Shelf shelf in availableNeighbors)
        {
            NeighborShelves.Add(shelf);
        }

        NeighborMessage = NeighborShelves.Count == 0
            ? "Der er ingen ledige naboreoler i den valgte periode."
            : $"Ledige naboreoler: {string.Join(", ", NeighborShelves.Select(shelf => shelf.ShelfNumber))}.";
    }
}