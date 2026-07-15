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

        public StatisticsWindow(int userId)
        {
            InitializeComponent();

            this.userId = userId;
            apiService = new ApiService();

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;
        }

        private async void OnWindowLoaded(
            object sender,
            RoutedEventArgs e)
        {
            var stats = await apiService.GetUserStatsAsync(userId);

            if (stats.HasValue)
            {
                SuccessRateText.Text = $"{stats.Value.successRate}%";
                SuccessProgressBar.Value = stats.Value.successRate;
                CompletedText.Text = stats.Value.completed.ToString();
                FailedText.Text = stats.Value.failed.ToString();
            }
        }

        private void OnCloseButtonClicked(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
        }
    }
}
