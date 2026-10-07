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
    private Tenant? _selectedTenant;
    private ObservableCollection<Tenant> _tenants;
    private ObservableCollection<Rental> _rentals;
    private readonly TenantRepository _tenantRepository = new();
    private readonly RentalRepository _rentalRepository = new();
    private readonly ShelfRepository _shelfRepository = new();
    public ICommand AddTenantCommand { get; }
    public ICommand DeleteTenantCommand { get; }
    public ICommand AddRentalCommand { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
    public Tenant? SelectedTenant
    {
        get => _selectedTenant;
        set => SetProperty(ref _selectedTenant, value);
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
        DeleteTenantCommand = new RelayCommand(ExecuteDeleteTenant);
        AddRentalCommand = new RelayCommand(ExecuteAddRental);

        LoadTenants();
        LoadRentals();
    }

    private void LoadTenants()
    {
        try
        {
            Tenants.Clear();

            foreach (Tenant tenant in _tenantRepository.GetAll())
            {
                Tenants.Add(tenant);
            }

            StatusMessage = $"{Tenants.Count} lejere indlæst fra databasen.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente lejere: {ex.Message}";
        }
    }
    private void LoadRentals()
    {
        try
        {
            Rentals.Clear();

            foreach (Rental rental in _rentalRepository.GetAll())
            {
                Rentals.Add(rental);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente lejeaftaler: {ex.Message}";
        }
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
                    Name = viewModel.Name,
                    Email = viewModel.Email,
                    Phone = viewModel.Phone
                };

                try
                {
                    _tenantRepository.Add(newTenant);
                    Tenants.Add(newTenant);
                    StatusMessage = $"Lejeren '{newTenant.Name}' blev gemt.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Kunne ikke gemme lejeren: {ex.Message}";
                }
            }
        }
    }
    private void ExecuteDeleteTenant(object? parameter)
    {
        Tenant? tenantToDelete = SelectedTenant;

        if (tenantToDelete == null)
        {
            StatusMessage = "Vælg en lejer, der skal slettes.";
            return;
        }

        try
        {
            _tenantRepository.Delete(tenantToDelete.TenantId);
            Tenants.Remove(tenantToDelete);
            SelectedTenant = null;
            StatusMessage = $"Lejeren '{tenantToDelete.Name}' blev slettet.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke slette lejeren: {ex.Message}";
        }
    }

    private void ExecuteAddRental(object? parameter)
    {
        List<Shelf> shelves;

        try
        {
            shelves = _shelfRepository.GetAll();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente reoler: {ex.Message}";
            return;
        }

        var window = new AddRentalWindow(Tenants, shelves, Rentals);

        if (window.ShowDialog() != true)
        {
            return;
        }

        var viewModel = window.DataContext as AddRentalViewModel;

        if (viewModel == null ||
            viewModel.TenantId <= 0 ||
            viewModel.ShelfId <= 0)
        {
            StatusMessage = "Vælg en gyldig lejer og reol.";
            return;
        }

        if (viewModel.EndDate.HasValue &&
            viewModel.EndDate.Value.Date < viewModel.StartDate.Date)
        {
            StatusMessage = "Slutdatoen må ikke være før startdatoen.";
            return;
        }

        try
        {
            bool shelfIsAvailable = _rentalRepository.IsShelfAvailable(
                viewModel.ShelfId,
                viewModel.StartDate,
                viewModel.EndDate);

            if (!shelfIsAvailable)
            {
                MessageBox.Show(
                    "Reolen er allerede lejet i den valgte periode.",
                    "Reolen er ikke ledig",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                StatusMessage =
                    "Reolen er allerede lejet i den valgte periode.";
                return;
            }

            var tenant = Tenants.FirstOrDefault(
                t => t.TenantId == viewModel.TenantId);

            var newRental = new Rental
            {
                TenantId = viewModel.TenantId,
                Tenant = tenant,
                ShelfId = viewModel.ShelfId,
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                MonthlyRent = viewModel.MonthlyRent,
                PaymentMethod = viewModel.SelectedPaymentMethod
            };

            _rentalRepository.AddAndRepriceActiveRentals(newRental);

            foreach (Rental existingRental in Rentals)
            {
                if (existingRental.TenantId == newRental.TenantId &&
                    existingRental.IsActive(newRental.StartDate.Date))
                {
                    existingRental.MonthlyRent = newRental.MonthlyRent;
                }
            }

            Rentals.Add(newRental);

            StatusMessage =
                $"Lejeaftalen blev gemt med ID {newRental.RentalId}.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Kunne ikke gemme lejeaftalen: {ex.Message}";
        }
    }
}

