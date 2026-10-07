using System.Collections.ObjectModel;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class SettlementViewModel : ViewModelBase
{
    private readonly TenantRepository _tenantRepository = new();
    private readonly SettlementRepository _settlementRepository = new();
    private Tenant? _selectedTenant;
    private DateTime? _selectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    private Settlement? _currentSettlement;
    private string _statusMessage = string.Empty;

    public ObservableCollection<Tenant> Tenants { get; } = new();

    public Tenant? SelectedTenant
    {
        get => _selectedTenant;
        set
        {
            if (SetProperty(ref _selectedTenant, value))
            {
                CurrentSettlement = null;
            }
        }
    }

    public DateTime? SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth, value))
            {
                CurrentSettlement = null;
            }
        }
    }

    public Settlement? CurrentSettlement
    {
        get => _currentSettlement;
        set => SetProperty(ref _currentSettlement, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand CalculateCommand { get; }
    public ICommand SaveCommand { get; }

    public SettlementViewModel()
    {
        CalculateCommand = new RelayCommand(_ => Calculate());
        SaveCommand = new RelayCommand(_ => Save());
        LoadTenants();
    }

    private void LoadTenants()
    {
        try
        {
            foreach (Tenant tenant in _tenantRepository.GetAll())
            {
                Tenants.Add(tenant);
            }
            StatusMessage = $"{Tenants.Count} lejere indlæst.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente lejere: {ex.Message}";
        }
    }

    private void Calculate()
    {
        if (SelectedTenant is null)
        {
            StatusMessage = "Vælg en lejer.";
            return;
        }

        if (SelectedMonth is null)
        {
            StatusMessage = "Vælg en måned.";
            return;
        }

        try
        {
            CurrentSettlement = _settlementRepository.Calculate(
                SelectedTenant.TenantId,
                SelectedMonth.Value);
            CurrentSettlement.Tenant = SelectedTenant;
            StatusMessage = "Afregningen er beregnet. Kontrollér beløbene før gemning.";
        }
        catch (Exception ex)
        {
            CurrentSettlement = null;
            StatusMessage = $"Kunne ikke beregne afregningen: {ex.Message}";
        }
    }

    private void Save()
    {
        if (CurrentSettlement is null)
        {
            StatusMessage = "Beregn afregningen først.";
            return;
        }

        if (CurrentSettlement.IsFinalized)
        {
            StatusMessage = "Denne afregning er allerede gemt.";
            return;
        }

        try
        {
            CurrentSettlement.SettlementId =
                _settlementRepository.Save(CurrentSettlement);
            CurrentSettlement.IsFinalized = true;
            OnPropertyChanged(nameof(CurrentSettlement));
            StatusMessage = "Afregningen blev gemt.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke gemme afregningen: {ex.Message}";
        }
    }
}