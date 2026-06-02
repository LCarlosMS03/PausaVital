using System;
using System.Windows;
using System.Windows.Threading;
using PausaVital.Services;

namespace PausaVital.Views
{
    public partial class MainWindow : Window
    {
        private readonly ApiService apiService;
        private readonly DispatcherTimer idleTimer;

        public MainWindow()
        {
            InitializeComponent();
            apiService = new ApiService();

            VerifyBackendConnection();

            // Configuramos el reloj para que "haga tic" cada 1 segundo
            idleTimer = new DispatcherTimer();
            idleTimer.Interval = TimeSpan.FromSeconds(1);
            idleTimer.Tick += OnIdleTimerTicked;
            idleTimer.Start();
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

        private void OnIdleTimerTicked(object? sender, EventArgs e)
        {
            // Le preguntamos al monitor nativo de Windows el tiempo inactivo
            TimeSpan idleTime = ActivityMonitor.GetIdleTime();

            // Actualizamos la ventana (F0 quita los decimales)
            IdleTimeText.Text = $"Idle Time: {idleTime.TotalSeconds:F0} seconds";
        }

        private void OnHideToTrayButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}