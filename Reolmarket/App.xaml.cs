using System.Windows;
using Reolmarket.Infrastructure;
using Reolmarket.Views;

namespace Reolmarket;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            DatabaseInitializer.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Programmet kunne ikke oprette eller klargøre databasen. " +
                "Kontrollér, at SQL Server Database Engine er installeret og " +
                "kører, og at din Windows-bruger har adgang til at oprette databaser.\n\n" +
                $"Detaljer: {ex.Message}",
                "Database kunne ikke startes",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
            return;
        }

        MainWindow = new MainWindow();
        MainWindow.Show();
    }
}
