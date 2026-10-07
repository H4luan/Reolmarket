using System.Collections.Generic;
using System.Windows;
using Reolmarket.Domain;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class AddRentalWindow : Window
{
    public AddRentalWindow(
    IEnumerable<Tenant> tenants,
    IEnumerable<Shelf> shelves,
    IEnumerable<Rental> existingRentals)
    {
        InitializeComponent();

        var viewModel = new AddRentalViewModel(
            tenants,
            shelves,
            existingRentals);

        DataContext = viewModel;

        viewModel.RequestClose += () =>
        {
            DialogResult = viewModel.DialogResult;
            Close();
        };
    }
}