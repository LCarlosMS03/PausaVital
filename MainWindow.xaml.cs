using System.Windows;

namespace PausaVital.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnHideToTrayButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}