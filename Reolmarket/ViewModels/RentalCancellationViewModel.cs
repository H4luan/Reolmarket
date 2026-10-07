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
        new DateTime(RequestDate.Year, RequestDate.Month, 1).AddMonths(1);

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
            .AddMonths(2)
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

        if (RequestDate.Day >= 20)
        {
            StatusMessage = "Casens opsigelsesregel gælder, når opsigelsen gives før den 20. i måneden.";
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