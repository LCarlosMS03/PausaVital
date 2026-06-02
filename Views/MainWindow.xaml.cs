using System;
using System.Threading.Tasks;
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

        private async void UpdateConnectionStatus(bool isConnected)
        {
            if (isConnected)
            {
                ConnectionStatusText.Text = "Backend: Connected";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Green;

                await UpdateStreakDisplayAsync();
            }
            else
            {
                ConnectionStatusText.Text = "Backend: Disconnected";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private async Task UpdateStreakDisplayAsync()
        {
            int streak = await apiService.GetCurrentStreakAsync();
            StreakText.Text = $"Current Streak: {streak} Breaks";
        }

        private async void OnIdleTimerTicked(object? sender, EventArgs e)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan elapsed = now - lastTickTime;
            lastTickTime = now;

            TimeSpan idleTime = ActivityMonitor.GetIdleTime();
            IdleTimeText.Text = $"Idle Time: {idleTime.TotalSeconds:F0} seconds";

            TimeSpan currentWork = breakManager.WorkTime;
            WorkTimeText.Text = $"Work Time: {currentWork.Minutes:D2}:{currentWork.Seconds:D2}";

            if (breakManager.ShouldTakeBreak(idleTime, elapsed))
            {
                App.TrayManager?.ShowNotification(
                    "20-20-20 Rule",
                    "Look at something 20 feet away for 20 seconds!");

                bool recorded = await apiService.RecordBreakAsync(1);
                if (recorded)
                {
                    await UpdateStreakDisplayAsync();
                }
            }
        }

        private async void OnTestBreakButtonClicked(object sender, RoutedEventArgs e)
        {
            bool recorded = await apiService.RecordBreakAsync(1);
            if (recorded)
            {
                await UpdateStreakDisplayAsync();
                App.TrayManager?.ShowNotification("Test Mode", "Break recorded successfully via test button!");
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