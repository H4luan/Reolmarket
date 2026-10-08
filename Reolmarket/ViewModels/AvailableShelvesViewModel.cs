using System.Collections.ObjectModel;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class AvailableShelvesViewModel : ViewModelBase
{
    private readonly ShelfRepository _shelfRepository = new();
    private readonly RentalRepository _rentalRepository = new();
    private DateTime? _selectedDate = DateTime.Today;
    private string _statusMessage = string.Empty;

    public ObservableCollection<AvailableShelfInfo> Shelves { get; } = new();

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            DateTime? date = value?.Date ?? DateTime.Today;
            if (SetProperty(ref _selectedDate, date))
            {
                LoadAvailableShelves();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public AvailableShelvesViewModel()
    {
        LoadAvailableShelves();
    }

    private void LoadAvailableShelves()
    {
        try
        {
            Shelves.Clear();
            DateTime selectedDate = SelectedDate?.Date ?? DateTime.Today;
            List<Shelf> allShelves = _shelfRepository.GetAll();
            List<Rental> allRentals = _rentalRepository.GetAll();

            foreach (Shelf shelf in allShelves
                .Where(shelf => !allRentals.Any(rental =>
                    rental.ShelfId == shelf.ShelfId &&
                    rental.IsActive(selectedDate)))
                .OrderBy(shelf => shelf.ShelfNumber))
            {
                Shelves.Add(new AvailableShelfInfo
                {
                    ShelfNumber = shelf.ShelfNumber,
                    Comment = string.IsNullOrWhiteSpace(shelf.Comment)
                        ? "—"
                        : shelf.Comment,
                    LayoutDescription = shelf.Layout switch
                    {
                        ShelfLayout.SixShelves => "6 hylder",
                        ShelfLayout.ThreeShelvesAndRail => "3 hylder + bøjlestang",
                        _ => "Ukendt indretning"
                    }
                });
            }

            StatusMessage = $"{Shelves.Count} ledige reoler pr. {selectedDate:dd-MM-yyyy}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente ledige reoler: {ex.Message}";
        }
    }
}
