using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;
using Reolmarket.Views;

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
        var window = new AddTenantWindow();
        if (window.ShowDialog() == true)
        {
            var viewModel = window.DataContext as AddTenantViewModel;
            if (viewModel != null)
            {
                var newTenant = new Tenant
                {
                    TenantId = Tenants.Count + 1,
                    Name = viewModel.Name,
                    Email = viewModel.Email,
                    Phone = viewModel.Phone
                };

                Tenants.Add(newTenant);
                StatusMessage = $"Tenant '{newTenant.Name}' added successfully";
            }
        }
    }

    private void ExecuteAddRental(object? parameter)
    {
        var window = new AddRentalWindow();
        if (window.ShowDialog() == true)
        {
            var viewModel = window.DataContext as AddRentalViewModel;
            if (viewModel != null && viewModel.TenantId > 0 && viewModel.ShelfId > 0)
            {
                var tenant = Tenants.FirstOrDefault(t => t.TenantId == viewModel.TenantId);

                var newRental = new Rental
                {
                    RentalId = Rentals.Count + 1,
                    TenantId = viewModel.TenantId,
                    Tenant = tenant,
                    ShelfId = viewModel.ShelfId,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    MonthlyRent = viewModel.MonthlyRent
                };

                Rentals.Add(newRental);
                StatusMessage = $"Rental added successfully for tenant ID {newRental.TenantId}";
            }
        }
    }
}
