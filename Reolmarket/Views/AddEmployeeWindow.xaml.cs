using System.Windows;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class AddEmployeeWindow : Window
{
    public AddEmployeeWindow()
    {
        InitializeComponent();

        var viewModel = new AddEmployeeViewModel();
        DataContext = viewModel;

        viewModel.RequestClose += () =>
        {
            DialogResult = viewModel.DialogResult;
            Close();
        };
    }
}