using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class SettlementViewModel : ViewModelBase
{
    private readonly TenantRepository _tenantRepository = new();
    private readonly SettlementRepository _settlementRepository = new();
    private Tenant? _selectedTenant;
    private DateTime? _selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private Settlement? _currentSettlement;
    private Settlement? _selectedSettlement;
    private Employee? _selectedEmployee;
    private string _statusMessage = string.Empty;

    public ObservableCollection<Tenant> Tenants { get; } = new();
    public ObservableCollection<Employee> Employees { get; } = new();
    public ObservableCollection<Settlement> Settlements { get; } = new();

    public Tenant? SelectedTenant
    {
        get => _selectedTenant;
        set
        {
            if (SetProperty(ref _selectedTenant, value))
            {
                CurrentSettlement = null;
                SelectedSettlement = null;
                LoadHistory();
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
        set
        {
            if (SetProperty(ref _currentSettlement, value))
            {
                OnPropertyChanged(nameof(HasCurrentSettlement));
                OnPropertyChanged(nameof(IsEditingExistingSettlement));
            }
        }
    }

    public Settlement? SelectedSettlement
    {
        get => _selectedSettlement;
        set
        {
            if (SetProperty(ref _selectedSettlement, value) && value is not null)
            {
                SelectedMonth = value.PeriodStart;
                CurrentSettlement = value;
                StatusMessage = "Gemt afregning valgt. Ret beløbene, vælg medarbejder, og gem ændringerne.";
            }
        }
    }

    public Employee? SelectedEmployee
    {
        get => _selectedEmployee;
        set => SetProperty(ref _selectedEmployee, value);
    }

    public bool HasCurrentSettlement => CurrentSettlement is not null;
    public bool IsEditingExistingSettlement => CurrentSettlement?.SettlementId > 0;

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
        LoadEmployees();
    }

    private void LoadTenants()
    {
        try
        {
            foreach (Tenant tenant in _tenantRepository.GetAll()) Tenants.Add(tenant);
            StatusMessage = $"{Tenants.Count} lejere indlæst.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente lejere: {ex.Message}";
        }
    }

    private void LoadEmployees()
    {
        try
        {
            foreach (Employee employee in new EmployeeRepository().GetAll()) Employees.Add(employee);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente medarbejdere: {ex.Message}";
        }
    }

    private void LoadHistory()
    {
        Settlements.Clear();
        if (SelectedTenant is null) return;

        try
        {
            foreach (Settlement settlement in _settlementRepository.GetForTenant(SelectedTenant.TenantId))
                Settlements.Add(settlement);
            StatusMessage = $"{Settlements.Count} afregninger fundet for {SelectedTenant.Name}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente afregningshistorikken: {ex.Message}";
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
            CurrentSettlement = _settlementRepository.Calculate(SelectedTenant.TenantId, SelectedMonth.Value);
            CurrentSettlement.Tenant = SelectedTenant;
            SelectedSettlement = null;
            StatusMessage = "Beløbene er beregnet, men endnu ikke gemt. Du kan rette felterne og derefter gemme afregningen.";
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
            StatusMessage = "Beregn en afregning eller vælg en fra historikken først.";
            return;
        }

        try
        {
            if (CurrentSettlement.SettlementId > 0)
            {
                if (SelectedEmployee is null)
                {
                    StatusMessage = "Vælg den medarbejder, der foretager rettelsen.";
                    return;
                }

                MessageBoxResult editConfirmation = MessageBox.Show(
                    $"Vil du gemme rettelserne til afregningen for {CurrentSettlement.PeriodStart:MMMM yyyy}? Rettelsen registreres med tidspunkt og medarbejder.",
                    "Gem rettelse af afregning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (editConfirmation != MessageBoxResult.Yes)
                {
                    StatusMessage = "Rettelsen blev ikke gemt.";
                    return;
                }

                _settlementRepository.Update(CurrentSettlement.SettlementId, CurrentSettlement, SelectedEmployee.EmployeeId);
                int savedId = CurrentSettlement.SettlementId;
                LoadHistory();
                SelectedSettlement = Settlements.FirstOrDefault(item => item.SettlementId == savedId);
                StatusMessage = $"Afregningen blev rettet af {SelectedEmployee.Name} og gemt med tidspunkt.";
            }
            else
            {
                Settlement? existing = Settlements.FirstOrDefault(item => item.PeriodEnd.Date == CurrentSettlement.PeriodEnd.Date);
                if (existing is not null)
                {
                    if (SelectedEmployee is null)
                    {
                        StatusMessage = "Der findes allerede en afregning for perioden. Vælg medarbejder, hvis den skal erstattes.";
                        return;
                    }
                    MessageBoxResult replaceConfirmation = MessageBox.Show(
                        $"Der findes allerede en afregning for {CurrentSettlement.PeriodStart:MMMM yyyy}. Vil du erstatte den med denne beregning? Rettelsen registreres med tidspunkt og medarbejder.",
                        "Erstat gemt afregning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                    if (replaceConfirmation != MessageBoxResult.Yes)
                    {
                        StatusMessage = "Den gemte afregning blev ikke ændret.";
                        return;
                    }
                    CurrentSettlement.SettlementId = existing.SettlementId;
                    _settlementRepository.Update(existing.SettlementId, CurrentSettlement, SelectedEmployee.EmployeeId);
                    int savedId = existing.SettlementId;
                    LoadHistory();
                    SelectedSettlement = Settlements.FirstOrDefault(item => item.SettlementId == savedId);
                    StatusMessage = $"Afregningen blev opdateret af {SelectedEmployee.Name}.";
                }
                else
                {
                    MessageBoxResult saveConfirmation = MessageBox.Show(
                        $"Vil du gemme månedsafregningen for {SelectedTenant?.Name} for {CurrentSettlement.PeriodStart:MMMM yyyy}?\n\n{CurrentSettlement.ResultMessage}\n\nAfregningen bliver gemt i lejerens historik.",
                        "Bekræft ny afregning",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.No);

                    if (saveConfirmation != MessageBoxResult.Yes)
                    {
                        StatusMessage = "Afregningen blev ikke gemt.";
                        return;
                    }

                    CurrentSettlement.SettlementId = _settlementRepository.Save(CurrentSettlement);
                    CurrentSettlement.IsFinalized = true;
                    LoadHistory();
                    SelectedSettlement = Settlements.FirstOrDefault(item => item.SettlementId == CurrentSettlement.SettlementId);
                    StatusMessage = "Afregningen blev gemt og vises nu i historikken.";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke gemme afregningen: {ex.Message}";
        }
    }
}
