using System.Collections.ObjectModel;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class RentalCancellationViewModel : ViewModelBase
{
    private readonly RentalRepository _rentalRepository = new();
    private Rental? _selectedRental;
    private DateTime _requestDate = DateTime.Today;
    private DateTime? _endDate = GetDefaultEndDate(DateTime.Today);
    private string _statusMessage = string.Empty;

    public ObservableCollection<Rental> Rentals { get; } = new();

    public Rental? SelectedRental
    {
        get => _selectedRental;
        set => SetProperty(ref _selectedRental, value);
    }

    public DateTime RequestDate
    {
        get => _requestDate;
        set
        {
            if (SetProperty(ref _requestDate, value))
            {
                EndDate = GetDefaultEndDate(value);
                OnPropertyChanged(nameof(EarliestEndDate));
            }
        }
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    public DateTime EarliestEndDate =>
        RequestDate.Day >= 20
            ? GetDefaultEndDate(RequestDate)
            : new DateTime(RequestDate.Year, RequestDate.Month, 1).AddMonths(1);

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand SaveCancellationCommand { get; }
    public ICommand ReloadCommand { get; }

    public RentalCancellationViewModel()
    {
        SaveCancellationCommand = new RelayCommand(_ => SaveCancellation());
        ReloadCommand = new RelayCommand(_ => LoadRentals());
        LoadRentals();
    }

    private static DateTime GetDefaultEndDate(DateTime requestDate) =>
        new DateTime(requestDate.Year, requestDate.Month, 1)
            .AddMonths(requestDate.Day >= 20 ? 3 : 2)
            .AddDays(-1);

    private void LoadRentals()
    {
        try
        {
            Rentals.Clear();
            foreach (Rental rental in _rentalRepository.GetAll())
            {
                Rentals.Add(rental);
            }
            StatusMessage = $"{Rentals.Count} lejeaftaler indlæst.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente lejeaftaler: {ex.Message}";
        }
    }

    private void SaveCancellation()
    {
        if (SelectedRental is null)
        {
            LoadRentals();
            StatusMessage = "Vælg den reol, der skal opsiges.";
            return;
        }

        if (!SelectedRental.IsActive(RequestDate.Date))
        {
            StatusMessage = "Den valgte lejeaftale er ikke aktiv på opsigelsesdatoen.";
            return;
        }

        if (EndDate is null || EndDate.Value.Date < EarliestEndDate.Date)
        {
            StatusMessage = $"Sidste lejedag skal være mindst {EarliestEndDate:dd-MM-yyyy}.";
            return;
        }

        bool submittedAfterDeadline = RequestDate.Day >= 20;
        string confirmationMessage = submittedAfterDeadline
            ? $"Opsigelsen er indgivet den {RequestDate:dd-MM-yyyy}, efter fristen den 20. Slutdatoen flyttes derfor til {EndDate:dd-MM-yyyy}, én måned senere. Leje skal betales frem til og med slutdatoen. Vil du gemme opsigelsen for {SelectedRental.Tenant?.Name}, reol {SelectedRental.Shelf?.ShelfNumber}?"
            : $"Er du sikker på, at du vil opsige reol {SelectedRental.Shelf?.ShelfNumber} for {SelectedRental.Tenant?.Name}? Slutdatoen bliver {EndDate:dd-MM-yyyy}, og lejeaftalen gælder til og med den dato.";

        System.Windows.MessageBoxResult confirmation = System.Windows.MessageBox.Show(
            confirmationMessage,
            "Bekræft opsigelse",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question,
            System.Windows.MessageBoxResult.No);

        if (confirmation != System.Windows.MessageBoxResult.Yes)
        {
            StatusMessage = "Opsigelsen blev ikke gemt.";
            return;
        }

        try
        {
            SelectedRental.EndDate = EndDate.Value.Date;
            _rentalRepository.Update(SelectedRental);
            StatusMessage =
                $"Reol {SelectedRental.Shelf?.ShelfNumber} er opsagt pr. {EndDate:dd-MM-yyyy}. " +
                "Lejeaftalen forbliver aktiv til og med denne dato.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke opsige reolen: {ex.Message}";
        }
    }
}
