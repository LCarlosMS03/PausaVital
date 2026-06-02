using System.Windows;
using PausaVital.Services;

namespace PausaVital.Views
{
    public partial class MainWindow : Window
    {
        private readonly ApiService apiService;

        public MainWindow()
        {
            InitializeComponent();
            apiService = new ApiService();
            
            VerifyBackendConnection();
        }

        private async void VerifyBackendConnection()
        {
            bool isConnected = await apiService.CheckHealthAsync();
            
            if (isConnected)
            {
                ConnectionStatusText.Text = "Backend: Connected";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                ConnectionStatusText.Text = "Backend: Disconnected";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void OnHideToTrayButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}