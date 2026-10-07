using System.Windows;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class ProductReturnsWindow : Window
{
    public ProductReturnsWindow()
    {
        InitializeComponent();
        DataContext = new ProductReturnViewModel();
    }
}