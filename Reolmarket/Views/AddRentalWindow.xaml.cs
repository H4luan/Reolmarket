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
    IEnumerable<Rental> existingRentals,
    Tenant? selectedTenant = null)
    {
        InitializeComponent();

        var viewModel = new AddRentalViewModel(
            tenants,
            shelves,
            existingRentals);

        DataContext = viewModel;
        if (selectedTenant is not null)
        {
            viewModel.TenantId = selectedTenant.TenantId;
        }

        viewModel.RequestClose += () =>
        {
            DialogResult = viewModel.DialogResult;
            Close();
        };
    }
}

