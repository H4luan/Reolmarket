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

    private void OpenShelves_Click(object sender, RoutedEventArgs e)
    {
        var shelvesWindow = new ShelvesWindow
        {
            Owner = this
        };

        shelvesWindow.ShowDialog();
    }
}
