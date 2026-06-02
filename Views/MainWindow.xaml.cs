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

        // Gamification Variables
        private bool isResting = false;
        private int restSecondsRemaining = 0;
        private const int RestDurationSeconds = 20;

        // Dynamic Profile Variables
        private int currentUserId = 0;
        private int currentHabitId = 0;

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

                // Initialize Dynamic Profile using Windows Session Name
                currentUserId = await apiService.LoginAsync(Environment.UserName);
                currentHabitId = await apiService.GetDefaultHabitAsync();

                await UpdateStreakAndShieldsAsync();
            }
            else
            {
                ConnectionStatusText.Text = "Backend: Disconnected";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private async Task UpdateStreakAndShieldsAsync()
        {
            if (currentUserId == 0) return;

            int streak = await apiService.GetCurrentStreakAsync(currentUserId);
            StreakText.Text = $"Current Streak: {streak} Breaks";

            int shields = await apiService.GetShieldsAsync(currentUserId);
            ShieldsText.Text = $"Shields: {shields}";
        }

        private async void OnIdleTimerTicked(object? sender, EventArgs e)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan elapsed = now - lastTickTime;
            lastTickTime = now;

            TimeSpan idleTime = ActivityMonitor.GetIdleTime();
            IdleTimeText.Text = $"Idle Time: {idleTime.TotalSeconds:F0} seconds";

            if (isResting)
            {
                if (idleTime.TotalSeconds < 1.0)
                {
                    await HandleFailedRestAsync();
                }
                else
                {
                    restSecondsRemaining--;
                    RestStatusText.Text = $"RESTING... DO NOT MOVE! ({restSecondsRemaining}s)";

                    if (restSecondsRemaining <= 0)
                    {
                        await HandleSuccessfulRestAsync();
                    }
                }
                return;
            }

            TimeSpan currentWork = breakManager.WorkTime;
            WorkTimeText.Text = $"Work Time: {currentWork.Minutes:D2}:{currentWork.Seconds:D2}";

            if (breakManager.ShouldTakeBreak(idleTime, elapsed))
            {
                StartRestMode();
            }
        }

        private void StartRestMode()
        {
            isResting = true;
            restSecondsRemaining = RestDurationSeconds;

            RestStatusText.Visibility = Visibility.Visible;
            RestStatusText.Text = $"RESTING... DO NOT MOVE! ({restSecondsRemaining}s)";
            RestStatusText.Foreground = System.Windows.Media.Brushes.DarkOrange;

            App.TrayManager?.ShowNotification(
                "20-20-20 Rule",
                $"Look away for {RestDurationSeconds} seconds! Do not touch the mouse or keyboard.");
        }

        private async Task HandleSuccessfulRestAsync()
        {
            isResting = false;
            RestStatusText.Visibility = Visibility.Collapsed;

            bool recorded = await apiService.RecordBreakAsync(currentUserId, currentHabitId, "completed");
            if (recorded)
            {
                await UpdateStreakAndShieldsAsync();
                App.TrayManager?.ShowNotification("Break Completed", "Great job! Streak updated.");
            }
        }

        private async Task HandleFailedRestAsync()
        {
            isResting = false;

            bool shieldConsumed = await apiService.ConsumeShieldAsync(currentUserId);

            if (shieldConsumed)
            {
                RestStatusText.Text = "STREAK SAVED BY SHIELD!";
                RestStatusText.Foreground = System.Windows.Media.Brushes.DodgerBlue;
                App.TrayManager?.ShowNotification("Shield Used!", "You moved, but a shield saved your streak!");
            }
            else
            {
                RestStatusText.Text = "STREAK BROKEN!";
                RestStatusText.Foreground = System.Windows.Media.Brushes.Red;
                App.TrayManager?.ShowNotification("Streak Broken", "You moved before the 20 seconds were up!");

                await apiService.RecordBreakAsync(currentUserId, currentHabitId, "failed");
            }

            await UpdateStreakAndShieldsAsync();

            await Task.Delay(3000);
            if (!isResting)
            {
                RestStatusText.Visibility = Visibility.Collapsed;
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