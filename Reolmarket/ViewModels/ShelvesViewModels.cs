using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class ShelvesViewModel : ViewModelBase
{
    private readonly ShelfRepository _repository = new();
    private Shelf? _selectedShelf;
    private string _statusMessage = string.Empty;

    public ObservableCollection<Shelf> Shelves { get; } = new();

    public ShelfLayoutOption[] LayoutOptions { get; } =
    [
        new(ShelfLayout.SixShelves, "6 hylder"),
        new(ShelfLayout.ThreeShelvesAndRail, "3 hylder + bøjlestang")
    ];

    public Shelf? SelectedShelf
    {
        get => _selectedShelf;
        set => SetProperty(ref _selectedShelf, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand SaveShelfCommand { get; }
    public ICommand ReloadShelvesCommand { get; }

    public ShelvesViewModel()
    {
        SaveShelfCommand = new RelayCommand(_ => SaveSelectedShelf());
        ReloadShelvesCommand = new RelayCommand(_ => LoadShelves());
        LoadShelves();
    }

    private void LoadShelves()
    {
        try
        {
            SelectedShelf = null;
            Shelves.Clear();

            foreach (Shelf shelf in _repository.GetAll())
            {
                Shelves.Add(shelf);
            }

            StatusMessage = $"{Shelves.Count} faste reoler indlæst.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente reoler: {ex.Message}";
        }
    }

    private void SaveSelectedShelf()
    {
        if (SelectedShelf is null)
        {
            StatusMessage = "Vælg en reol først.";
            return;
        }

        try
        {
            if (SelectedShelf.ShelfId <= 0)
            {
                StatusMessage = "Reolregisteret indeholder de 80 faste reoler. Vælg en eksisterende reol.";
                return;
            }

            _repository.Update(SelectedShelf);
            LoadShelves();
            StatusMessage = "Reolens placering og indretning er gemt.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke gemme reolen: {ex.Message}";
        }
    }
}

public sealed record ShelfLayoutOption(ShelfLayout Value, string DisplayName);
