using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class ProductReturnViewModel : ViewModelBase
{
    private readonly ProductReturnRepository _repository = new();
    private ReturnableSaleLine? _selectedSaleLine;
    private int _quantityToReturn = 1;
    private string _comment = string.Empty;
    private string _statusMessage = string.Empty;

    public ObservableCollection<ReturnableSaleLine> SaleLines { get; } = new();

    public ReturnableSaleLine? SelectedSaleLine
    {
        get => _selectedSaleLine;
        set => SetProperty(ref _selectedSaleLine, value);
    }

    public int QuantityToReturn
    {
        get => _quantityToReturn;
        set => SetProperty(ref _quantityToReturn, value);
    }

    public string Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand SaveReturnCommand { get; }
    public ICommand ReloadCommand { get; }

    public ProductReturnViewModel()
    {
        SaveReturnCommand = new RelayCommand(_ => SaveReturn());
        ReloadCommand = new RelayCommand(_ => LoadSaleLines());
        LoadSaleLines();
    }

    private void LoadSaleLines()
    {
        try
        {
            SaleLines.Clear();
            foreach (ReturnableSaleLine line in _repository.GetReturnableSaleLines())
            {
                SaleLines.Add(line);
            }

            if (SelectedSaleLine is not null &&
                !SaleLines.Any(line => line.SaleLineId == SelectedSaleLine.SaleLineId))
            {
                SelectedSaleLine = null;
            }

            StatusMessage = $"{SaleLines.Count} salgslinjer kan returneres.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente salg: {ex.Message}";
        }
    }

    private void SaveReturn()
    {
        if (SelectedSaleLine is null)
        {
            StatusMessage = "Vælg en vare fra et tidligere salg.";
            return;
        }

        if (QuantityToReturn <= 0 ||
            QuantityToReturn > SelectedSaleLine.RemainingQuantity)
        {
            StatusMessage = "Indtast et gyldigt returantal.";
            return;
        }

        decimal refundAmount = SelectedSaleLine.SalePrice * QuantityToReturn;
        MessageBoxResult confirmation = MessageBox.Show(
            $"Vil du registrere retur af {QuantityToReturn} stk. '{SelectedSaleLine.ProductName}'?\n\nReturbeløb: {refundAmount:N2} kr.\nLageret bliver øget med {QuantityToReturn} stk.",
            "Bekræft retur",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);

        if (confirmation != MessageBoxResult.Yes)
        {
            StatusMessage = "Returen blev ikke gemt.";
            return;
        }

        try
        {
            _repository.AddReturn(
                SelectedSaleLine.SaleLineId,
                QuantityToReturn,
                string.IsNullOrWhiteSpace(Comment) ? null : Comment.Trim());

            QuantityToReturn = 1;
            Comment = string.Empty;
            LoadSaleLines();
            StatusMessage = "Returen blev gemt, og lageret blev opdateret.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke gemme returen: {ex.Message}";
        }
    }
}
