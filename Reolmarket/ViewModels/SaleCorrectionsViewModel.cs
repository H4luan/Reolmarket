using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class SaleCorrectionsViewModel : ViewModelBase
{
    private readonly SaleCorrectionRepository _repository = new();
    private SaleLineCorrection? _selectedLine;
    private string _statusMessage = string.Empty;

    public ObservableCollection<SaleLineCorrection> Lines { get; } = new();
    public ObservableCollection<Shelf> Shelves { get; } = new();

    public SaleLineCorrection? SelectedLine
    {
        get => _selectedLine;
        set => SetProperty(ref _selectedLine, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand ReloadCommand { get; }

    public SaleCorrectionsViewModel()
    {
        SaveCommand = new RelayCommand(_ => Save());
        ReloadCommand = new RelayCommand(_ => Load());
        Load();
    }

    private void Load()
    {
        try
        {
            Lines.Clear();
            Shelves.Clear();

            foreach (Shelf shelf in new ShelfRepository().GetAll())
            {
                Shelves.Add(shelf);
            }

            foreach (SaleLineCorrection line in _repository.GetAll())
            {
                Lines.Add(line);
            }

            StatusMessage = $"{Lines.Count} salgslinjer indlæst.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente salg: {ex.Message}";
        }
    }

    private void Save()
    {
        if (SelectedLine is null)
        {
            StatusMessage = "Vælg en salgslinje.";
            return;
        }

        int savedLineId = SelectedLine.SaleLineId;

        try
        {
            _repository.Update(SelectedLine);
            Load();
            SelectedLine = Lines.FirstOrDefault(
                line => line.SaleLineId == savedLineId);
            StatusMessage = "Salgslinjen blev gemt, og salgets total blev genberegnet.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke gemme rettelsen: {ex.Message}";
        }
    }
}
