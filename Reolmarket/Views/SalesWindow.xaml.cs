using System.Windows;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class SalesWindow : Window
{
    public SalesWindow()
    {
        InitializeComponent();
        DataContext = new SalesViewModel();
    }
}