using System.Windows;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class AddRentalWindow : Window
{
    public AddRentalWindow()
    {
        InitializeComponent();
        var viewModel = new AddRentalViewModel();
        DataContext = viewModel;

        viewModel.RequestClose += () =>
        {
            DialogResult = viewModel.DialogResult;
            Close();
        };
    }
}
