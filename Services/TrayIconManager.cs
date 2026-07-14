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

        private readonly ToolStripMenuItem showMenuItem;
        private readonly ToolStripMenuItem statsMenuItem;
        private readonly ToolStripMenuItem exitMenuItem;

        public TrayIconManager(Window window)
        {
            dashboardWindow = window;

            contextMenu = new ContextMenuStrip();

            showMenuItem = new ToolStripMenuItem();
            showMenuItem.Font = new Font(showMenuItem.Font, WinDrawingFontStyle.Bold);
            showMenuItem.Click += (s, e) => ShowDashboard();

            statsMenuItem = new ToolStripMenuItem();
            statsMenuItem.Click += (s, e) => ShowStatisticsWindow();

            exitMenuItem = new ToolStripMenuItem();
            exitMenuItem.Click += (s, e) => ExitApplication();

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

            systemTrayIcon.DoubleClick += (s, e) => ShowDashboard();

            RefreshTexts();
        }

        public void RefreshTexts()
        {
            showMenuItem.Text = TranslationManager.Get("MenuShow", "Show Dashboard");
            statsMenuItem.Text = TranslationManager.Get("MenuStats", "Advanced Statistics");
            exitMenuItem.Text = TranslationManager.Get("MenuExit", "Exit");
        }

        private void ShowStatisticsWindow()
        {
            if (dashboardWindow is MainWindow mainWindow)
            {
                if (mainWindow.currentUserId == 0)
                {
                    ShowNotification(
                        TranslationManager.Get("NotReadyTitle", "Not Ready"),
                        TranslationManager.Get("BackendNotReady", "Please wait for the backend to connect."));

                    return;
                }

                var statsWindow = new StatisticsWindow(mainWindow.currentUserId);
                statsWindow.ShowDialog();
            }
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
            contextMenu.Dispose();
            systemTrayIcon.Dispose();
        }
    }
}