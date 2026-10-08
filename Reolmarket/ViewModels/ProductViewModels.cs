using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;

namespace Reolmarket.ViewModels;

public class ProductsViewModel : ViewModelBase
{
    private readonly ProductRepository _productRepository = new();
    private readonly ShelfRepository _shelfRepository = new();

    private Product? _selectedProduct;
    private string _statusMessage = string.Empty;

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Shelf> Shelves { get; } = new();

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set => SetProperty(ref _selectedProduct, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand AddProductCommand { get; }
    public ICommand SaveProductCommand { get; }
    public ICommand DeleteProductCommand { get; }
    public ICommand ReloadProductsCommand { get; }

    public ProductsViewModel()
    {
        AddProductCommand = new RelayCommand(_ => AddProduct());
        SaveProductCommand = new RelayCommand(_ => SaveProduct());
        DeleteProductCommand = new RelayCommand(_ => DeleteProduct());
        ReloadProductsCommand = new RelayCommand(_ => LoadData());

        LoadData();
    }

    private void LoadData()
    {
        try
        {
            SelectedProduct = null;
            Products.Clear();
            Shelves.Clear();

            foreach (Shelf shelf in _shelfRepository.GetAll())
            {
                Shelves.Add(shelf);
            }

            foreach (Product product in _productRepository.GetAll())
            {
                Products.Add(product);
            }

            StatusMessage = $"{Products.Count} produkter indlæst.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente data: {ex.Message}";
        }
    }

    private void AddProduct()
    {
        Shelf? shelf = Shelves.FirstOrDefault();

        if (shelf is null)
        {
            StatusMessage = "Opret mindst én reol, før du tilføjer produkter.";
            return;
        }

        var product = new Product
        {
            Name = string.Empty,
            Barcode = string.Empty,
            Price = 0,
            StockQuantity = 0,
            ShelfId = shelf.ShelfId
        };

        Products.Add(product);
        SelectedProduct = product;
        StatusMessage = "Udfyld produktets oplysninger, og vælg Gem.";
    }

    private void SaveProduct()
    {
        if (SelectedProduct is null)
        {
            StatusMessage = "Vælg eller opret et produkt først.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedProduct.Name) ||
            string.IsNullOrWhiteSpace(SelectedProduct.Barcode) ||
            SelectedProduct.Price <= 0 ||
            SelectedProduct.StockQuantity < 0 ||
            SelectedProduct.ShelfId <= 0)
        {
            StatusMessage =
                "Udfyld navn, stregkode, en pris over 0, lagerantal på mindst 0 og vælg en reol.";
            return;
        }

        try
        {
            if (SelectedProduct.ProductId == 0)
            {
                _productRepository.Add(SelectedProduct);
            }
            else
            {
                _productRepository.Update(SelectedProduct);
            }

            LoadData();
            StatusMessage = "Produktet er gemt.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke gemme produktet: {ex.Message}";
        }
    }

    private void DeleteProduct()
    {
        if (SelectedProduct is null)
        {
            StatusMessage = "Vælg et produkt først.";
            return;
        }

        bool isSavedProduct = SelectedProduct.ProductId > 0;
        string confirmationMessage = isSavedProduct
            ? $"Er du sikker på, at du vil slette produktet '{SelectedProduct.Name}' fra databasen? Handlingen kan ikke fortrydes. Produkter, der indgår i et tidligere salg, kan ikke slettes."
            : $"Er du sikker på, at du vil fjerne det nye produkt '{SelectedProduct.Name}' fra listen? Det er endnu ikke gemt i databasen.";

        MessageBoxResult confirmation = MessageBox.Show(
            confirmationMessage,
            "Bekræft sletning af produkt",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (confirmation != MessageBoxResult.Yes)
        {
            StatusMessage = "Sletningen blev annulleret.";
            return;
        }

        try
        {
            if (SelectedProduct.ProductId == 0)
            {
                Products.Remove(SelectedProduct);
                SelectedProduct = null;
                StatusMessage = "Det nye produkt blev fjernet fra listen.";
                return;
            }

            int productId = SelectedProduct.ProductId;
            _productRepository.Delete(productId);
            LoadData();
            StatusMessage = "Produktet er slettet.";
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            StatusMessage = "Produktet indgår i et tidligere salg og kan derfor ikke slettes. Hvis varen ikke længere sælges, kan du sætte lagerantallet til 0 og gemme produktet.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke slette produktet: {ex.Message}";
        }
    }
}
