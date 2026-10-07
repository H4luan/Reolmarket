using System.Windows;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class ProductsWindow : Window
{
    public ProductsWindow()
    {
        InitializeComponent();
        DataContext = new ProductsViewModel();
    }
}