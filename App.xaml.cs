using System.Windows;
using PausaVital.Services;
using PausaVital.Views;

namespace PausaVital
{
    public partial class App : System.Windows.Application
    {
        private TrayIconManager? trayManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow appWindow = new MainWindow();

            trayManager = new TrayIconManager(appWindow);

            appWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayManager?.Dispose();
            base.OnExit(e);
        }
    }
}