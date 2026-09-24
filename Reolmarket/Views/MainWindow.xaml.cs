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
}
