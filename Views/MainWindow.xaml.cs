using System;
using System.Windows;
using System.Windows.Threading;
using PausaVital.Services;

namespace PausaVital.Views
{
    public partial class MainWindow : Window
    {
        private readonly ApiService apiService;
        private readonly BackendProcessManager backendProcessManager;
        private readonly DispatcherTimer idleTimer;
        private readonly BreakManager breakManager;
        private DateTime lastTickTime = DateTime.UtcNow;

        public MainWindow()
        {
            InitializeComponent();

            apiService = new ApiService();
            backendProcessManager = new BackendProcessManager(apiService);
            breakManager = new BreakManager();

            Loaded += OnMainWindowLoaded;
            Closed += OnMainWindowClosed;

            // Setup timer to tick every 1 second.
            idleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            idleTimer.Tick += OnIdleTimerTicked;
            idleTimer.Start();
        }

        private async void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            ConnectionStatusText.Text = "Backend: Starting...";
            ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Goldenrod;

            bool isConnected = await backendProcessManager.EnsureBackendIsRunningAsync();
            UpdateConnectionStatus(isConnected);
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
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
            DateTime now = DateTime.UtcNow;
            TimeSpan elapsed = now - lastTickTime;
            lastTickTime = now;

            // Get idle time from Windows.
            TimeSpan idleTime = ActivityMonitor.GetIdleTime();
            IdleTimeText.Text = $"Idle Time: {idleTime.TotalSeconds:F0} seconds";

            // Check if it's time for a 20-20-20 break.
            if (breakManager.ShouldTakeBreak(idleTime, elapsed))
            {
                App.TrayManager?.ShowNotification(
                    "20-20-20 Rule",
                    "Look at something 20 feet away for 20 seconds!");
            }
        }

        private void OnHideToTrayButtonClicked(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void OnMainWindowClosed(object? sender, EventArgs e)
        {
            idleTimer.Stop();
            backendProcessManager.Dispose();
        }
    }
}
