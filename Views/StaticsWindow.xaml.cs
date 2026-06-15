using System;
using System.Windows;
using System.Windows.Threading;
using PausaVital.Services;

namespace PausaVital.Views
{
    public partial class StatisticsWindow : Window
    {
        private readonly ApiService apiService;
        private readonly int userId;
        private readonly DispatcherTimer liveTimer; // Timer for live stats

        public StatisticsWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.apiService = new ApiService();

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed; // Clean up when closed

            // Setup the live timer
            liveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            liveTimer.Tick += OnLiveTimerTicked;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Fetch static data from SQLite
            var stats = await apiService.GetUserStatsAsync(userId);

            if (stats.HasValue)
            {
                SuccessRateText.Text = $"{stats.Value.successRate}%";
                SuccessProgressBar.Value = stats.Value.successRate;

                CompletedText.Text = stats.Value.completed.ToString();
                FailedText.Text = stats.Value.failed.ToString();
            }

            // Start live monitoring
            liveTimer.Start();
        }

        // Update the idle text every second
        private void OnLiveTimerTicked(object? sender, EventArgs e)
        {
            TimeSpan idle = ActivityMonitor.GetIdleTime();
            LiveIdleText.Text = $"{(int)idle.TotalMinutes:D2}:{idle.Seconds:D2}";
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Stop the timer to free up memory
        private void OnWindowClosed(object? sender, EventArgs e)
        {
            liveTimer.Stop();
        }
    }
}