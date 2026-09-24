using System.Windows;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class AddTenantWindow : Window
{
    public AddTenantWindow()
    {
        InitializeComponent();
        var viewModel = new AddTenantViewModel();
        DataContext = viewModel;

        viewModel.RequestClose += () =>
        {
            DialogResult = viewModel.DialogResult;
            Close();
        };
    }
}
