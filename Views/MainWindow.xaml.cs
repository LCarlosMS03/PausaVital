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

        private bool isResting = false;
        private int restSecondsRemaining = 0;
        private const int RestDurationSeconds = 20;

        public int currentUserId { get; private set; } = 0;
        private int currentHabitId = 0;

        private int cachedStreak = 0;
        private int cachedShields = 0;

        public bool IsShuttingDown { get; set; } = false;
        private bool hasShownMinimizeNotification = false;

        public MainWindow()
        {
            InitializeComponent();

            apiService = new ApiService();
            backendProcessManager = new BackendProcessManager(apiService);
            breakManager = new BreakManager();

            Loaded += OnMainWindowLoaded;
            Closed += OnMainWindowClosed;

            Closing += OnMainWindowClosing;
            StateChanged += OnWindowStateChanged;

            idleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            idleTimer.Tick += OnIdleTimerTicked;
            idleTimer.Start();
        }

        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsShuttingDown) return;

            e.Cancel = true;

            var prefs = PreferencesManager.Load();

            if (prefs.CloseAction == "Minimize")
            {
                ExecuteHideToTray();
                return;
            }
            if (prefs.CloseAction == "Exit")
            {
                IsShuttingDown = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            var prompt = new ClosePromptWindow { Owner = this };
            if (prompt.ShowDialog() == true)
            {
                if (prompt.SelectedAction == "Minimize")
                {
                    ExecuteHideToTray();
                }
                else if (prompt.SelectedAction == "Exit")
                {
                    IsShuttingDown = true;
                    System.Windows.Application.Current.Shutdown();
                }
            }
        }

        private void ExecuteHideToTray()
        {
            Hide();
            if (!hasShownMinimizeNotification)
            {
                App.TrayManager?.ShowNotification("Pausa Vital", "Running in background. Right-click tray icon to exit.");
                hasShownMinimizeNotification = true;
            }
        }

        private void OnHideToTrayButtonClicked(object sender, RoutedEventArgs e)
        {
            ExecuteHideToTray();
        }

        private async void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            UpdateConnectionUI("Starting connection...", System.Windows.Media.Brushes.Goldenrod);
            bool isConnected = await backendProcessManager.EnsureBackendIsRunningAsync();
            UpdateConnectionStatus(isConnected);
        }

        private void UpdateConnectionUI(string text, System.Windows.Media.Brush color)
        {
            ConnectionStatusText.Text = text;
            ConnectionStatusDot.Fill = color;
        }

        private async void UpdateConnectionStatus(bool isConnected)
        {
            if (isConnected)
            {
                UpdateConnectionUI("Connected", System.Windows.Media.Brushes.MediumSeaGreen);

                currentUserId = await apiService.LoginAsync(Environment.UserName);
                currentHabitId = await apiService.GetDefaultHabitAsync();
                await UpdateStreakAndShieldsAsync();
            }
            else
            {
                UpdateConnectionUI("Disconnected", System.Windows.Media.Brushes.IndianRed);
            }
        }

        private async Task UpdateStreakAndShieldsAsync()
        {
            if (currentUserId == 0) return;

            cachedStreak = await apiService.GetCurrentStreakAsync(currentUserId);
            StreakText.Text = $"🔥 Streak: {cachedStreak}";

            cachedShields = await apiService.GetShieldsAsync(currentUserId);
            ShieldsText.Text = $"🛡️ Shields: {cachedShields}";
        }

        private async void OnIdleTimerTicked(object? sender, EventArgs e)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan elapsed = now - lastTickTime;
            lastTickTime = now;

            TimeSpan idleTime = ActivityMonitor.GetIdleTime();

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

                    App.TrayManager?.UpdateText($"Resting: {restSecondsRemaining}s left");

                    if (restSecondsRemaining <= 0)
                    {
                        await HandleSuccessfulRestAsync();
                    }
                }
                return;
            }

            TimeSpan currentWork = breakManager.WorkTime;
            WorkTimeText.Text = $"{currentWork.Minutes:D2}:{currentWork.Seconds:D2}";

            string trayHoverText = $"Work: {currentWork.Minutes:D2}:{currentWork.Seconds:D2} | Streak: {cachedStreak}";
            App.TrayManager?.UpdateText(trayHoverText);

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
                RestStatusText.Text = "SAVED BY SHIELD!";
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

        private void OnMainWindowClosed(object? sender, EventArgs e)
        {
            idleTimer.Stop();
            backendProcessManager.Dispose();
        }
    }
}