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
        private readonly DispatcherTimer liveTimer;

        public StatisticsWindow(int userId)
        {
            InitializeComponent();

            this.userId = userId;
            apiService = new ApiService();

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;

            liveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            liveTimer.Tick += OnLiveTimerTicked;
        }

        private async void OnWindowLoaded(
            object sender,
            RoutedEventArgs e)
        {
            var stats = await apiService.GetUserStatsAsync(userId);

            if (stats.HasValue)
            {
                SuccessRateText.Text =
                    $"{stats.Value.successRate}%";

                SuccessProgressBar.Value =
                    stats.Value.successRate;

                CompletedText.Text =
                    stats.Value.completed.ToString();

                FailedText.Text =
                    stats.Value.failed.ToString();
            }

            liveTimer.Start();
        }

        private void OnLiveTimerTicked(
            object? sender,
            EventArgs e)
        {
            TimeSpan idleTime = ActivityMonitor.GetIdleTime();

            LiveIdleText.Text =
                $"{(int)idleTime.TotalMinutes:D2}:{idleTime.Seconds:D2}";
        }

        private void OnCloseButtonClicked(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        private void OnWindowClosed(
            object? sender,
            EventArgs e)
        {
            liveTimer.Stop();
        }
    }
}