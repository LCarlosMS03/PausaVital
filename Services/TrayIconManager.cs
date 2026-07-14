using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using PausaVital.Views;
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

            var showMenuItem = new ToolStripMenuItem
            {
                Text = "Mostrar panel"
            };

            showMenuItem.Font = new Font(
                showMenuItem.Font,
                WinDrawingFontStyle.Bold);

            showMenuItem.Click += (_, _) => ShowDashboard();

            var statsMenuItem = new ToolStripMenuItem
            {
                Text = "Estadísticas avanzadas"
            };

            statsMenuItem.Click += (_, _) => ShowStatisticsWindow();

            var exitMenuItem = new ToolStripMenuItem
            {
                Text = "Salir"
            };

            exitMenuItem.Click += (_, _) => ExitApplication();

            contextMenu.Items.Add(showMenuItem);
            contextMenu.Items.Add(statsMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitMenuItem);

            systemTrayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Visible = true,
                Text = "Pausa Vital",
                ContextMenuStrip = contextMenu
            };

            systemTrayIcon.DoubleClick += (_, _) => ShowDashboard();
        }

        private void ShowStatisticsWindow()
        {
            if (dashboardWindow is not MainWindow mainWindow)
            {
                return;
            }

            if (mainWindow.currentUserId == 0)
            {
                ShowNotification(
                    "Sistema no disponible",
                    "Espera a que el programa termine de conectarse antes de abrir las estadísticas.");

                return;
            }

            var statsWindow = new StatisticsWindow(mainWindow.currentUserId)
            {
                Owner = mainWindow
            };

            statsWindow.ShowDialog();
        }

        public void UpdateText(string text)
        {
            const int maxLength = 63;

            if (string.IsNullOrWhiteSpace(text))
            {
                systemTrayIcon.Text = "Pausa Vital";
                return;
            }

            systemTrayIcon.Text = text.Length > maxLength
                ? text[..maxLength]
                : text;
        }

        public void ShowNotification(string title, string message)
        {
            systemTrayIcon.ShowBalloonTip(
                3000,
                title,
                message,
                ToolTipIcon.Info);
        }

        private void ShowDashboard()
        {
            dashboardWindow.ShowInTaskbar = true;
            dashboardWindow.Show();
            dashboardWindow.WindowState = WindowState.Normal;
            dashboardWindow.Activate();
        }

        private void ExitApplication()
        {
            if (dashboardWindow is MainWindow mainWindow)
            {
                mainWindow.IsShuttingDown = true;
            }

            System.Windows.Application.Current.Shutdown();
        }

        public void Dispose()
        {
            systemTrayIcon.Visible = false;
            contextMenu.Dispose();
            systemTrayIcon.Dispose();
        }
    }
}