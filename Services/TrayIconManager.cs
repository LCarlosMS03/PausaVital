using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace PausaVital.Services
{
    public class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon systemTrayIcon;
        private readonly Window dashboardWindow;

        public TrayIconManager(Window window)
        {
            dashboardWindow = window;

            systemTrayIcon = new NotifyIcon();
            systemTrayIcon.Icon = SystemIcons.Information;
            systemTrayIcon.Visible = true;
            systemTrayIcon.Text = "Pausa Vital";

            systemTrayIcon.DoubleClick += OnTrayIconDoubleClicked;
        }

        private void OnTrayIconDoubleClicked(object? sender, EventArgs e)
        {
            dashboardWindow.Show();
            dashboardWindow.WindowState = WindowState.Normal;
        }

        public void Dispose()
        {
            systemTrayIcon.Dispose();
        }
    }
}
