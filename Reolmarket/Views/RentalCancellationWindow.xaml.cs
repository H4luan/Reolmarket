using System.Windows;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class RentalCancellationWindow : Window
{
    public RentalCancellationWindow()
    {
        InitializeComponent();
        DataContext = new RentalCancellationViewModel();
    }
}