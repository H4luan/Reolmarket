using Reolmarket.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Reolmarket.Views
{
    /// <summary>
    /// Interaction logic for ShelvesWindow.xaml
    /// </summary>
    public partial class ShelvesWindow : Window
    {
        public ShelvesWindow()
        {
            InitializeComponent();
            DataContext = new ShelvesViewModel();
        }
    }
}
