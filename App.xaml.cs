using System.Windows;
using PausaVital.Services;
using PausaVital.Views;

namespace PausaVital
{
    public partial class App : System.Windows.Application
    {
        public static TrayIconManager? TrayManager { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow appWindow = new MainWindow();

            TrayManager = new TrayIconManager(appWindow);

            appWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            TrayManager?.Dispose();
            TrayManager = null;
            base.OnExit(e);
        }
    }
}
