using System.Collections.Generic;
using System.Windows;
using Reolmarket.Domain;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class AddRentalWindow : Window
{
    public AddRentalWindow(
        IEnumerable<Tenant> tenants,
        IEnumerable<Shelf> shelves)
    {
        InitializeComponent();

        var viewModel = new AddRentalViewModel(tenants, shelves);
        DataContext = viewModel;

        viewModel.RequestClose += () =>
        {
            DialogResult = viewModel.DialogResult;
            Close();
        };
    }
}