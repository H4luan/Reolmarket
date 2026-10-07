using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        public Array LayoutOptions { get; } =
        Enum.GetValues(typeof(ShelfLayout));

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

        public ICommand AddShelfCommand { get; }
        public ICommand SaveShelfCommand { get; }
        public ICommand DeleteShelfCommand { get; }
        public ICommand ReloadShelvesCommand { get; }

        public ShelvesViewModel()
        {
            AddShelfCommand = new RelayCommand(_ => AddShelf());
            SaveShelfCommand = new RelayCommand(_ => SaveSelectedShelf());
            DeleteShelfCommand = new RelayCommand(_ => DeleteSelectedShelf());
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

                StatusMessage = $"{Shelves.Count} reoler indlæst.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Kunne ikke hente reoler: {ex.Message}";
            }
        }

        private void AddShelf()
        {
            var usedNumbers = Shelves
                .Select(shelf => shelf.ShelfNumber)
                .ToHashSet();

            int nextNumber = Enumerable
                .Range(1, 80)
                .FirstOrDefault(number => !usedNumbers.Contains(number));

            if (nextNumber == 0)
            {
                StatusMessage = "Alle 80 reolnumre er allerede brugt.";
                return;
            }

            var shelf = new Shelf
            {
                ShelfNumber = nextNumber,
                Layout = ShelfLayout.SixShelves,
                Location = string.Empty
            };

            Shelves.Add(shelf);
            SelectedShelf = shelf;
            StatusMessage = "Ny reol oprettet. Udfyld oplysningerne, og vælg Gem.";
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
                if (SelectedShelf.ShelfId == 0)
                {
                    _repository.Add(SelectedShelf);
                }
                else
                {
                    _repository.Update(SelectedShelf);
                }

                LoadShelves();
                StatusMessage = "Reolen er gemt.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Kunne ikke gemme reolen: {ex.Message}";
            }
        }

        private void DeleteSelectedShelf()
        {
            if (SelectedShelf is null)
            {
                StatusMessage = "Vælg en reol først.";
                return;
            }

            try
            {
                if (SelectedShelf.ShelfId == 0)
                {
                    Shelves.Remove(SelectedShelf);
                    StatusMessage = "Den nye reol blev fjernet fra listen.";
                    SelectedShelf = null;
                    return;
                }

                int shelfId = SelectedShelf.ShelfId;
                _repository.Delete(shelfId);
                LoadShelves();
                StatusMessage = "Reolen er slettet.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Kunne ikke slette reolen: {ex.Message}";
            }
        }
    }