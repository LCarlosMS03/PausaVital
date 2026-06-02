using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using WinDrawingFontStyle = System.Drawing.FontStyle;

namespace PausaVital.Services
{
    public class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon systemTrayIcon;
        private readonly Window dashboardWindow;
        private readonly ContextMenuStrip contextMenu;

        public TrayIconManager(Window window)
        {
            dashboardWindow = window;

            contextMenu = new ContextMenuStrip();

            var showMenuItem = new ToolStripMenuItem("Show Dashboard");
            showMenuItem.Font = new Font(showMenuItem.Font, WinDrawingFontStyle.Bold);
            showMenuItem.Click += (s, e) => ShowDashboard();

            var statsMenuItem = new ToolStripMenuItem("Advanced Statistics");
            statsMenuItem.Click += (s, e) => ShowNotification("Coming Soon", "Advanced statistics will be available in the next update.");

            var exitMenuItem = new ToolStripMenuItem("Exit Pausa Vital");
            exitMenuItem.Click += (s, e) => ExitApplication();

            contextMenu.Items.Add(showMenuItem);
            contextMenu.Items.Add(statsMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitMenuItem);

            systemTrayIcon = new NotifyIcon();
            systemTrayIcon.Icon = SystemIcons.Information;
            systemTrayIcon.Visible = true;
            systemTrayIcon.Text = "Pausa Vital";
            systemTrayIcon.ContextMenuStrip = contextMenu;

            systemTrayIcon.DoubleClick += (s, e) => ShowDashboard();
        }

        public void UpdateText(string text)
        {
            if (text.Length >= 63)
            {
                text = text.Substring(0, 63);
            }
            systemTrayIcon.Text = text;
        }

        public void ShowNotification(string title, string message)
        {
            systemTrayIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }

        private void ShowDashboard()
        {
            dashboardWindow.Show();
            dashboardWindow.WindowState = WindowState.Normal;
            dashboardWindow.Activate();
        }

        // NEW: Safely authorize and trigger the shutdown
        private void ExitApplication()
        {
            // Authorize the main window to close
            if (dashboardWindow is PausaVital.Views.MainWindow mainWindow)
            {
                mainWindow.IsShuttingDown = true;
            }

            // Trigger shutdown
            System.Windows.Application.Current.Shutdown();
        }

        public void Dispose()
        {
            contextMenu.Dispose();
            systemTrayIcon.Dispose();
        }
    }
}