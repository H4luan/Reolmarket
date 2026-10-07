using System.Windows;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void OpenShelves_Click(object sender, RoutedEventArgs e)
    {
        var shelvesWindow = new ShelvesWindow
        {
            Owner = this
        };

        shelvesWindow.ShowDialog();
    }
    private void OpenProducts_Click(object sender, RoutedEventArgs e)
    {
        var productsWindow = new ProductsWindow
        {
            Owner = this
        };

        productsWindow.ShowDialog();
    }
    private void OpenSales_Click(object sender, RoutedEventArgs e)
    {
        var salesWindow = new SalesWindow
        {
            Owner = this
        };

        salesWindow.ShowDialog();
    }

    private void OpenReturns_Click(object sender, RoutedEventArgs e)
    {
        var returnsWindow = new ProductReturnsWindow
        {
            Owner = this
        };

        returnsWindow.ShowDialog();
    }
    private void OpenSettlement_Click(object sender, RoutedEventArgs e)
    {
        var settlementWindow = new SettlementWindow
        {
            Owner = this
        };

        settlementWindow.ShowDialog();
    }

    private void OpenRentalCancellation_Click(object sender, RoutedEventArgs e)
    {
        var cancellationWindow = new RentalCancellationWindow
        {
            Owner = this
        };

        cancellationWindow.ShowDialog();
    }
    private void OpenAvailableShelves_Click(object sender, RoutedEventArgs e)
    {
        var availableShelvesWindow = new AvailableShelvesWindow
        {
            Owner = this
        };

        availableShelvesWindow.ShowDialog();
    }
    private void OpenSaleCorrections_Click(object sender, RoutedEventArgs e)
    {
        var window = new SaleCorrectionsWindow { Owner = this };
        window.ShowDialog();
    }
}
