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
        private readonly DispatcherTimer reconnectTimer;
        private readonly DispatcherTimer hydrationTimer;
        private readonly BreakManager breakManager;

        private DateTime lastTickTime = DateTime.UtcNow;
        private bool isResting;
        private int restSecondsRemaining;
        private int restDurationSeconds = 20;
        private int currentHabitId;
        private int cachedStreak;
        private int cachedShields;
        private bool hasShownMinimizeNotification;

        public int currentUserId { get; private set; }
        public bool IsShuttingDown { get; set; }

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

            reconnectTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            reconnectTimer.Tick += OnReconnectTimerTicked;

            hydrationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromHours(1)
            };
            hydrationTimer.Tick += OnHydrationTimerTicked;
            hydrationTimer.Start();

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

        private void OnMainWindowClosing(
            object? sender,
            System.ComponentModel.CancelEventArgs e)
        {
            if (IsShuttingDown)
            {
                return;
            }

            e.Cancel = true;

            var preferences = PreferencesManager.Load();

            if (preferences.CloseAction == "Minimize")
            {
                ExecuteHideToTray();
                return;
            }

            if (preferences.CloseAction == "Exit")
            {
                IsShuttingDown = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            var prompt = new ClosePromptWindow
            {
                Owner = this
            };

            if (prompt.ShowDialog() != true)
            {
                return;
            }

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

        private void ExecuteHideToTray()
        {
            Hide();

            if (hasShownMinimizeNotification)
            {
                return;
            }

            App.TrayManager?.ShowNotification(
                "Pausa Vital",
                "La aplicación continúa ejecutándose en segundo plano. Haz clic derecho en el icono para salir.");

            hasShownMinimizeNotification = true;
        }

        private void OnHideToTrayButtonClicked(
            object sender,
            RoutedEventArgs e)
        {
            ExecuteHideToTray();
        }

        private async void OnMainWindowLoaded(
            object sender,
            RoutedEventArgs e)
        {
            var preferences = PreferencesManager.Load();

            if (string.IsNullOrWhiteSpace(preferences.SelectedMode) ||
                preferences.SelectedMode == "None")
            {
                var modeWindow = new ModeSelectionWindow
                {
                    Owner = this
                };

                bool? modeWasSelected = modeWindow.ShowDialog();

                if (modeWasSelected != true)
                {
                    System.Windows.Application.Current.Shutdown();
                    return;
                }

                preferences = PreferencesManager.Load();
            }

            string mode;

            if (preferences.SelectedMode == "Pomodoro")
            {
                mode = "Pomodoro";
            }
            else if (preferences.SelectedMode == "20-20-20")
            {
                mode = "20-20-20";
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "No se pudo determinar el modo de trabajo. Debes seleccionar uno para continuar.",
                    "Modo no seleccionado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                System.Windows.Application.Current.Shutdown();
                return;
            }

            breakManager.SetMode(mode);
            restDurationSeconds = mode == "Pomodoro" ? 300 : 20;

            UpdateConnectionUI(
                "Iniciando conexión...",
                System.Windows.Media.Brushes.Goldenrod);

            bool isConnected =
                await backendProcessManager.EnsureBackendIsRunningAsync();

            await UpdateConnectionStatusAsync(isConnected);
        }

        private async void OnReconnectTimerTicked(
            object? sender,
            EventArgs e)
        {
            reconnectTimer.Stop();

            UpdateConnectionUI(
                "Reintentando conexión...",
                System.Windows.Media.Brushes.Goldenrod);

            bool isConnected =
                await backendProcessManager.EnsureBackendIsRunningAsync();

            await UpdateConnectionStatusAsync(isConnected);
        }

        private void OnHydrationTimerTicked(
            object? sender,
            EventArgs e)
        {
            App.TrayManager?.ShowNotification(
                "Recordatorio de hidratación",
                "Es momento de tomar un vaso de agua para mantenerte saludable y concentrado.");
        }

        private void UpdateConnectionUI(
            string text,
            System.Windows.Media.Brush color)
        {
            ConnectionStatusText.Text = text;
            ConnectionStatusDot.Fill = color;
        }

        private async Task UpdateConnectionStatusAsync(bool isConnected)
        {
            if (isConnected)
            {
                reconnectTimer.Stop();

                UpdateConnectionUI(
                    "Conectado",
                    System.Windows.Media.Brushes.MediumSeaGreen);

                currentUserId =
                    await apiService.LoginAsync(Environment.UserName);

                currentHabitId =
                    await apiService.GetDefaultHabitAsync();

                await UpdateStreakAndShieldsAsync();
                return;
            }

            UpdateConnectionUI(
                "Desconectado. Reintentando...",
                System.Windows.Media.Brushes.IndianRed);

            reconnectTimer.Start();
        }

        private async Task UpdateStreakAndShieldsAsync()
        {
            if (currentUserId == 0)
            {
                return;
            }

            cachedStreak =
                await apiService.GetCurrentStreakAsync(currentUserId);

            StreakText.Text = $"🔥 Racha: {cachedStreak}";

            cachedShields =
                await apiService.GetShieldsAsync(currentUserId);

            ShieldsText.Text = $"🛡️ Escudos: {cachedShields}";
        }

        private async void OnIdleTimerTicked(
            object? sender,
            EventArgs e)
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

                    RestStatusText.Text =
                        $"DESCANSANDO... ¡NO TE MUEVAS! ({restSecondsRemaining}s)";

                    App.TrayManager?.UpdateText(
                        $"Descanso: {restSecondsRemaining}s restantes");

                    if (restSecondsRemaining <= 0)
                    {
                        await HandleSuccessfulRestAsync();
                    }
                }

                return;
            }

            TimeSpan currentWork = breakManager.WorkTime;

            WorkTimeText.Text =
                $"{currentWork.Minutes:D2}:{currentWork.Seconds:D2}";

            App.TrayManager?.UpdateText(
                $"Trabajo: {currentWork.Minutes:D2}:{currentWork.Seconds:D2} | Racha: {cachedStreak}");

            if (breakManager.ShouldTakeBreak(idleTime, elapsed))
            {
                StartRestMode();
            }
        }

        private void StartRestMode()
        {
            isResting = true;
            restSecondsRemaining = restDurationSeconds;

            RestStatusText.Visibility = Visibility.Visible;
            RestStatusText.Text =
                $"DESCANSANDO... ¡NO TE MUEVAS! ({restSecondsRemaining}s)";

            RestStatusText.Foreground =
                System.Windows.Media.Brushes.DarkOrange;

            App.TrayManager?.ShowNotification(
                "Hora de descansar",
                GetBreakTip());
        }

        private async Task HandleSuccessfulRestAsync()
        {
            isResting = false;
            RestStatusText.Visibility = Visibility.Collapsed;

            if (currentUserId == 0 || currentHabitId == 0)
            {
                App.TrayManager?.ShowNotification(
                    "Pausa Vital",
                    "El sistema todavía no está listo para registrar el descanso.");

                return;
            }

            bool recorded = await apiService.RecordBreakAsync(
                currentUserId,
                currentHabitId,
                "completed");

            if (!recorded)
            {
                return;
            }

            await UpdateStreakAndShieldsAsync();

            App.TrayManager?.ShowNotification(
                "Descanso completado",
                "¡Buen trabajo! Tu racha fue actualizada.");
        }

        private async Task HandleFailedRestAsync()
        {
            isResting = false;

            if (currentUserId == 0 || currentHabitId == 0)
            {
                RestStatusText.Visibility = Visibility.Collapsed;
                return;
            }

            bool shieldConsumed =
                await apiService.ConsumeShieldAsync(currentUserId);

            if (shieldConsumed)
            {
                RestStatusText.Text = "¡SALVADO POR UN ESCUDO!";
                RestStatusText.Foreground =
                    System.Windows.Media.Brushes.DodgerBlue;

                App.TrayManager?.ShowNotification(
                    "¡Escudo utilizado!",
                    "Te moviste antes de terminar, pero un escudo protegió tu racha.");
            }
            else
            {
                RestStatusText.Text = "¡RACHA TERMINADA!";
                RestStatusText.Foreground =
                    System.Windows.Media.Brushes.Red;

                App.TrayManager?.ShowNotification(
                    "Racha terminada",
                    "Te moviste antes de completar el descanso.");

                await apiService.RecordBreakAsync(
                    currentUserId,
                    currentHabitId,
                    "failed");
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
            reconnectTimer.Stop();
            hydrationTimer.Stop();
            backendProcessManager.Dispose();
        }

        private string GetBreakTip()
        {
            return restDurationSeconds == 300
                ? "¡Hora de tomar un descanso de 5 minutos! Levántate y estírate."
                : "Mira a lo lejos durante 20 segundos. No muevas el mouse ni uses el teclado.";
        }
    }
}
