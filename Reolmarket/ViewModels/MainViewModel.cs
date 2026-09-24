using System.Collections.ObjectModel;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string _statusMessage = "Ready";
    private ObservableCollection<Tenant> _tenants;
    private ObservableCollection<Rental> _rentals;

    public ICommand AddTenantCommand { get; }
    public ICommand AddRentalCommand { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ObservableCollection<Tenant> Tenants
    {
        get => _tenants;
        set => SetProperty(ref _tenants, value);
    }

    public ObservableCollection<Rental> Rentals
    {
        get => _rentals;
        set => SetProperty(ref _rentals, value);
    }

    public MainViewModel()
    {
        _tenants = new ObservableCollection<Tenant>();
        _rentals = new ObservableCollection<Rental>();

        AddTenantCommand = new RelayCommand(ExecuteAddTenant);
        AddRentalCommand = new RelayCommand(ExecuteAddRental);

        LoadSampleData();
    }

    private void LoadSampleData()
    {
        var tenant1 = new Tenant
        { 
            TenantId = 1, 
            Name = "John Doe", 
            Email = "john@example.com", 
            Phone = "123-456-7890" 
        };

        var tenant2 = new Tenant 
        { 
            TenantId = 2, 
            Name = "Jane Smith", 
            Email = "jane@example.com", 
            Phone = "098-765-4321" 
        };

        Tenants.Add(tenant1);
        Tenants.Add(tenant2);

        var rental1 = new Rental
        {
            RentalId = 1,
            TenantId = 1,
            Tenant = tenant1,
            ShelfId = 1,
            StartDate = DateTime.Now,
            MonthlyRent = 499.99m
        };

        Rentals.Add(rental1);

        StatusMessage = "Sample data loaded successfully";
    }

    private void ExecuteAddTenant(object? parameter)
    {
        // TODO: Implement logic to add a new tenant
        StatusMessage = "Add Tenant feature";
    }

    private void ExecuteAddRental(object? parameter)
    {
        StatusMessage = "Add Rental feature";
    }
}
