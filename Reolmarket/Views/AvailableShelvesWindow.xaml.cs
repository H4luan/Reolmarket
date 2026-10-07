using System.Windows;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class AvailableShelvesWindow : Window
{
    public AvailableShelvesWindow()
    {
        InitializeComponent();
        DataContext = new AvailableShelvesViewModel();
    }
}