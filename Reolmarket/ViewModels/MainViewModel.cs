using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;
using Reolmarket.Views;

namespace Reolmarket.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string _statusMessage = "Klar";
    private Tenant? _selectedTenant;
    private bool _useCustomRent;
    private decimal _customRentPerShelf;
    private ObservableCollection<Tenant> _tenants;
    private ObservableCollection<Rental> _rentals;
    private readonly TenantRepository _tenantRepository = new();
    private readonly RentalRepository _rentalRepository = new();
    private readonly SettlementRepository _settlementRepository = new();
    private readonly ShelfRepository _shelfRepository = new();
    public ICommand AddTenantCommand { get; }
    public ICommand DeleteTenantCommand { get; }
    public ICommand AddRentalCommand { get; }
    public ICommand SaveCustomRentCommand { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
    public Tenant? SelectedTenant
    {
        get => _selectedTenant;
        set
        {
            if (SetProperty(ref _selectedTenant, value))
            {
                UseCustomRent = value?.UseCustomRent ?? false;
                CustomRentPerShelf = value?.CustomRentPerShelf ?? 0m;
                OnPropertyChanged(nameof(HasSelectedTenant));
                LoadSelectedTenantDetails();
            }
        }
    }

    public bool HasSelectedTenant => SelectedTenant is not null;

    public bool UseCustomRent
    {
        get => _useCustomRent;
        set => SetProperty(ref _useCustomRent, value);
    }

    public decimal CustomRentPerShelf
    {
        get => _customRentPerShelf;
        set => SetProperty(ref _customRentPerShelf, value);
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

    public ObservableCollection<Rental> SelectedTenantRentals { get; } = new();
    public ObservableCollection<Settlement> SelectedTenantSettlements { get; } = new();

    public MainViewModel()
    {
        _tenants = new ObservableCollection<Tenant>();
        _rentals = new ObservableCollection<Rental>();

        AddTenantCommand = new RelayCommand(ExecuteAddTenant);
        DeleteTenantCommand = new RelayCommand(ExecuteDeleteTenant);
        AddRentalCommand = new RelayCommand(ExecuteAddRental);
        SaveCustomRentCommand = new RelayCommand(ExecuteSaveCustomRent);

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

    public void RefreshSelectedTenantDetails()
    {
        LoadSelectedTenantDetails();
    }

    private void LoadSelectedTenantDetails()
    {
        SelectedTenantRentals.Clear();
        SelectedTenantSettlements.Clear();

        if (SelectedTenant is null)
        {
            return;
        }

        foreach (Rental rental in Rentals.Where(r => r.TenantId == SelectedTenant.TenantId))
        {
            SelectedTenantRentals.Add(rental);
        }

        try
        {
            foreach (Settlement settlement in _settlementRepository.GetForTenant(SelectedTenant.TenantId))
            {
                SelectedTenantSettlements.Add(settlement);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente lejerens afregningshistorik: {ex.Message}";
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
                    SelectedTenant = newTenant;
                    StatusMessage = $"Lejeren '{newTenant.Name}' blev gemt.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Kunne ikke gemme lejeren: {ex.Message}";
                }
            }
        }
    }
    private void ExecuteSaveCustomRent(object? parameter)
    {
        Tenant? tenant = SelectedTenant;
        if (tenant is null)
        {
            StatusMessage = "Vælg en lejer først.";
            return;
        }

        if (UseCustomRent && CustomRentPerShelf <= 0m)
        {
            StatusMessage = "Særprisen skal være større end 0 kr. pr. reol pr. måned.";
            return;
        }

        string priceDescription = UseCustomRent
            ? $"Særprisen bliver {CustomRentPerShelf:N2} kr. pr. reol pr. måned"
            : "særprisen bliver slået fra, og standardprisen efter antal reoler bliver brugt";
        MessageBoxResult confirmation = MessageBox.Show(
            $"Vil du gemme ændringen for {tenant.Name}? {priceDescription}. " +
            "Prisen gælder for aktive reoler og fremtidige lejeaftaler, indtil den ændres igen.",
            "Bekræft ændring af lejepris",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);

        if (confirmation != MessageBoxResult.Yes)
        {
            StatusMessage = "Lejeprisen blev ikke ændret.";
            return;
        }

        try
        {
            var pricingUpdate = new Tenant
            {
                TenantId = tenant.TenantId,
                UseCustomRent = UseCustomRent,
                CustomRentPerShelf = UseCustomRent ? CustomRentPerShelf : 0m
            };
            _tenantRepository.UpdateRentPricing(pricingUpdate, DateTime.Today);
            tenant.UseCustomRent = pricingUpdate.UseCustomRent;
            tenant.CustomRentPerShelf = pricingUpdate.CustomRentPerShelf;
            LoadRentals();
            LoadSelectedTenantDetails();
            StatusMessage = UseCustomRent
                ? $"Særprisen for {tenant.Name} er gemt."
                : $"Særprisen for {tenant.Name} er slået fra.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke gemme lejeprisen: {ex.Message}";
        }
    }

    private void ExecuteDeleteTenant(object? parameter)
    {
        Tenant? tenantToDelete = SelectedTenant;

        if (tenantToDelete is null)
        {
            StatusMessage = "Vælg en lejer, der skal slettes.";
            return;
        }

        MessageBoxResult confirmation = MessageBox.Show(
            $"Er du sikker på, at du vil slette lejeren '{tenantToDelete.Name}'?\n\nLejerens lejeaftaler og afregningshistorik bliver også slettet. Salgs- og returhistorik bevares i butikkens salgshistorik.\n\nHandlingen kan ikke fortrydes.",
            "Bekræft sletning af lejer",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (confirmation != MessageBoxResult.Yes)
        {
            StatusMessage = "Sletning af lejeren blev annulleret.";
            return;
        }

        try
        {
            int tenantId = tenantToDelete.TenantId;
            _tenantRepository.Delete(tenantId);

            foreach (Rental rental in Rentals
                         .Where(r => r.TenantId == tenantId)
                         .ToList())
            {
                Rentals.Remove(rental);
            }

            Tenants.Remove(tenantToDelete);
            SelectedTenant = null;
            StatusMessage = $"Lejeren '{tenantToDelete.Name}', lejeaftaler og afregningshistorik blev slettet.";
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

        Tenant? requestedTenant = parameter as Tenant;
        var window = new AddRentalWindow(Tenants, shelves, Rentals, requestedTenant);

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
                Shelf = shelves.FirstOrDefault(s => s.ShelfId == viewModel.ShelfId),
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                MonthlyRent = viewModel.MonthlyRent,
                PaymentMethod = viewModel.SelectedPaymentMethod
            };

            List<Rental> rentalsAffectedByNewPrice = Rentals
                .Where(existingRental =>
                    existingRental.TenantId == newRental.TenantId &&
                    existingRental.IsActive(newRental.StartDate.Date))
                .ToList();

            string endDateText = newRental.EndDate.HasValue
                ? newRental.EndDate.Value.ToString("dd-MM-yyyy")
                : "ingen slutdato";
            string paymentMethodText = newRental.PaymentMethod switch
            {
                PaymentMethod.Cash => "Kontant",
                PaymentMethod.MobilePay => "MobilePay",
                PaymentMethod.Card => "Kort",
                _ => "Ukendt"
            };
            string repricingText = rentalsAffectedByNewPrice.Count == 0
                ? string.Empty
                : $"\n\nPrisen på {rentalsAffectedByNewPrice.Count} eksisterende aktive reol(er) ændres også til {newRental.MonthlyRent:N0} kr. pr. måned pr. reol.";

            MessageBoxResult confirmation = MessageBox.Show(
                $"Opret lejeaftale for {tenant?.Name}, reol {newRental.Shelf?.ShelfNumber}, fra {newRental.StartDate:dd-MM-yyyy} til {endDateText}?\n\nPris: {newRental.MonthlyRent:N0} kr. pr. måned pr. reol\nBetalingsform: {paymentMethodText}{repricingText}",
                "Bekræft lejeaftale",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            if (confirmation != MessageBoxResult.Yes)
            {
                StatusMessage = "Lejeaftalen blev ikke oprettet.";
                return;
            }

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
            LoadSelectedTenantDetails();

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





