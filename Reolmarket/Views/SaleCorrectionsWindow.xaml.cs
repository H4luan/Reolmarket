using System.Windows;
using Reolmarket.ViewModels;
namespace Reolmarket.Views;
public partial class SaleCorrectionsWindow : Window
{
    public SaleCorrectionsWindow(){InitializeComponent();DataContext=new SaleCorrectionsViewModel();}
    private void SaveCorrection_Click(object sender, RoutedEventArgs e)
    {
        CorrectionsGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Cell, true);
        CorrectionsGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

        if (DataContext is SaleCorrectionsViewModel viewModel)
        {
            viewModel.SaveCommand.Execute(null);
        }
    }
}
