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
            bool createdNew;

            appMutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                System.Windows.MessageBox.Show("Pausa Vital is already running in the background. Check your System Tray.",
                                "Pausa Vital",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            MainWindow appWindow = new MainWindow();
            TrayManager = new TrayIconManager(appWindow);
            appWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            TrayManager?.Dispose();
            TrayManager = null;

            if (appMutex != null)
            {
                appMutex.ReleaseMutex();
                appMutex.Dispose();
            }

            base.OnExit(e);
        }
    }
}