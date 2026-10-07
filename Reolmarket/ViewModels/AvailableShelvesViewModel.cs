using System.Collections.ObjectModel;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class AvailableShelvesViewModel : ViewModelBase
{
    private readonly ShelfRepository _shelfRepository = new();
    private readonly RentalRepository _rentalRepository = new();
    private string _statusMessage = string.Empty;

    public ObservableCollection<AvailableShelfInfo> Shelves { get; } = new();

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
            List<Shelf> allShelves = _shelfRepository.GetAll();
            List<Rental> allRentals = _rentalRepository.GetAll();

            foreach (Shelf shelf in allShelves
                .Where(shelf => !allRentals.Any(rental =>
                    rental.ShelfId == shelf.ShelfId &&
                    rental.IsActive(DateTime.Today)))
                .OrderBy(shelf => shelf.ShelfNumber))
            {
                Shelves.Add(new AvailableShelfInfo
                {
                    ShelfNumber = shelf.ShelfNumber,
                    Location = string.IsNullOrWhiteSpace(shelf.Location)
                        ? "Placering ikke angivet"
                        : shelf.Location,
                    LayoutDescription = shelf.Layout switch
                    {
                        ShelfLayout.SixShelves => "6 hylder",
                        ShelfLayout.ThreeShelvesAndRail => "3 hylder + bøjlestang",
                        _ => "Ukendt indretning"
                    }
                });
            }

            StatusMessage = $"{Shelves.Count} ledige reoler pr. dags dato.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente ledige reoler: {ex.Message}";
        }
    }
}