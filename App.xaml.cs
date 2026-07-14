using System.Linq;
using System.Threading;
using System.Windows;
using PausaVital.Services;
using PausaVital.Views;

namespace PausaVital
{
    public partial class App : System.Windows.Application
    {
        public static TrayIconManager? TrayManager { get; private set; }

        private static Mutex? appMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "PausaVital_SingleInstance_Mutex";

            appMutex = new Mutex(
                initiallyOwned: true,
                name: mutexName,
                createdNew: out bool createdNew);

            if (!createdNew)
            {
                System.Windows.MessageBox.Show(
                    "Pausa Vital ya se está ejecutando en segundo plano. Revisa el icono en la bandeja del sistema.",
                    "Pausa Vital",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            bool startInBackground = e.Args.Contains("--background");

            MainWindow appWindow = new MainWindow();
            TrayManager = new TrayIconManager(appWindow);

            if (startInBackground)
            {
                appWindow.ShowInTaskbar = false;
                appWindow.WindowState = WindowState.Minimized;
                appWindow.Show();
                appWindow.Hide();
            }
            else
            {
                appWindow.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            TrayManager?.Dispose();
            TrayManager = null;

            if (appMutex is not null)
            {
                appMutex.ReleaseMutex();
                appMutex.Dispose();
                appMutex = null;
            }

            base.OnExit(e);
        }
    }
}
