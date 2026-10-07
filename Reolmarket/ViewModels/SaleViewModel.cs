using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Reolmarket.Domain;
using Reolmarket.Infrastructure;
using Reolmarket.Views;
namespace Reolmarket.ViewModels;

public class SalesViewModel : ViewModelBase
{
    private readonly EmployeeRepository _employeeRepository = new();
    private readonly ProductRepository _productRepository = new();
    private readonly SaleRepository _saleRepository = new();

    private Employee? _selectedEmployee;
    private Product? _selectedProduct;
    private SaleLine? _selectedSaleLine;
    private int _quantityToAdd = 1;
    private decimal _cashReceived;
    private string _statusMessage = string.Empty;

    private Sale _currentSale = new()
    {
        SoldAt = DateTime.Now,
        PaymentMethod = PaymentMethod.Cash
    };

    public ObservableCollection<Employee> Employees { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<SaleLine> CartLines { get; } = new();

    public Employee? SelectedEmployee
    {
        get => _selectedEmployee;
        set => SetProperty(ref _selectedEmployee, value);
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set => SetProperty(ref _selectedProduct, value);
    }

    public SaleLine? SelectedSaleLine
    {
        get => _selectedSaleLine;
        set => SetProperty(ref _selectedSaleLine, value);
    }

    public int QuantityToAdd
    {
        get => _quantityToAdd;
        set => SetProperty(ref _quantityToAdd, value);
    }
    public decimal CashReceived
    {
        get => _cashReceived;
        set
        {
            if (SetProperty(ref _cashReceived, value))
            {
                OnPropertyChanged(nameof(ChangeDue));
            }
        }
    }

    public decimal ChangeDue => Math.Max(0m, CashReceived - CartTotal);

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public decimal CartTotal => _currentSale.TotalAmount;

    public ICommand AddToSaleCommand { get; }
    public ICommand RemoveFromSaleCommand { get; }
    public ICommand CompleteSaleCommand { get; }
    public ICommand ReloadCommand { get; }

    public ICommand AddEmployeeCommand { get; }

    public SalesViewModel()
    {
        AddToSaleCommand = new RelayCommand(_ => AddSelectedProduct());
        RemoveFromSaleCommand = new RelayCommand(_ => RemoveSelectedLine());
        CompleteSaleCommand = new RelayCommand(_ => CompleteSale());
        ReloadCommand = new RelayCommand(_ => LoadData());
        AddEmployeeCommand = new RelayCommand(_ => AddEmployee());

        LoadData();
    }

    private void LoadData()
    {
        try
        {
            Employees.Clear();
            Products.Clear();

            foreach (Employee employee in _employeeRepository.GetAll())
            {
                Employees.Add(employee);
            }

            foreach (Product product in _productRepository.GetAll())
            {
                Products.Add(product);
            }

            StatusMessage =
                $"{Products.Count} produkter og {Employees.Count} medarbejdere indlæst.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke hente data: {ex.Message}";
        }
    }

    private void AddSelectedProduct()
    {
        if (SelectedProduct is null)
        {
            StatusMessage = "Vælg et produkt først.";
            return;
        }

        try
        {
            _currentSale.AddProduct(SelectedProduct, QuantityToAdd);
            CartLines.Add(_currentSale.Lines.Last());
            OnPropertyChanged(nameof(CartTotal));
            OnPropertyChanged(nameof(ChangeDue));
            StatusMessage = "Produktet er føjet til salget.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private void RemoveSelectedLine()
    {
        if (SelectedSaleLine is null)
        {
            StatusMessage = "Vælg en salgslinje først.";
            return;
        }

        _currentSale.Lines.Remove(SelectedSaleLine);
        CartLines.Remove(SelectedSaleLine);
        SelectedSaleLine = null;

        OnPropertyChanged(nameof(CartTotal));
        OnPropertyChanged(nameof(ChangeDue));
        StatusMessage = "Produktet er fjernet fra salget.";
    }

    private void CompleteSale()
    {
        if (SelectedEmployee is null)
        {
            StatusMessage = "Vælg en medarbejder.";
            return;
        }

        if (CartLines.Count == 0)
        {
            StatusMessage = "Tilføj mindst ét produkt til salget.";
            return;
        }
        if (CashReceived < CartTotal)
        {
            StatusMessage = "Det modtagne beløb er mindre end totalen.";
            return;
        }

        try
        {
            _currentSale.EmployeeId = SelectedEmployee.EmployeeId;
            _currentSale.SoldAt = DateTime.Now;

            _saleRepository.Add(_currentSale);

            CartLines.Clear();
            _currentSale = new Sale
            {
                SoldAt = DateTime.Now,
                PaymentMethod = PaymentMethod.Cash
            };
            CashReceived = 0m;

            OnPropertyChanged(nameof(CartTotal));
            OnPropertyChanged(nameof(ChangeDue));
            LoadData();

            StatusMessage = "Salget blev gemt, og lageret blev opdateret.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke gemme salget: {ex.Message}";
        }
    }
    private void AddEmployee()
    {
        var window = new AddEmployeeWindow();

        if (window.ShowDialog() != true)
        {
            return;
        }

        if (window.DataContext is not AddEmployeeViewModel viewModel)
        {
            StatusMessage = "Kunne ikke hente medarbejderens oplysninger.";
            return;
        }

        var employee = new Employee
        {
            Name = viewModel.Name,
            Email = viewModel.Email,
            Phone = viewModel.Phone
        };

        try
        {
            _employeeRepository.Add(employee);
            Employees.Add(employee);
            SelectedEmployee = employee;
            StatusMessage = $"Medarbejderen '{employee.Name}' blev oprettet.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunne ikke oprette medarbejderen: {ex.Message}";
        }
    }
}