using System.Windows;
using Reolmarket.ViewModels;

namespace Reolmarket.Views;

public partial class SettlementWindow : Window
{
    public SettlementWindow()
    {
        InitializeComponent();
        DataContext = new SettlementViewModel();
    }
}