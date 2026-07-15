using PausaVital.Views;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using DrawingIcon = System.Drawing.Icon;
using DrawingImage = System.Drawing.Image;
using WinDrawingFontStyle = System.Drawing.FontStyle;

namespace PausaVital.Services
{
    public class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon systemTrayIcon;
        private readonly Window dashboardWindow;
        private readonly ContextMenuStrip contextMenu;

        private readonly DrawingIcon trayIcon;
        private readonly DrawingIcon statisticsIcon;
        private readonly DrawingIcon showPanelIcon;
        private readonly DrawingIcon exitIcon;

        private readonly DrawingImage statisticsMenuImage;
        private readonly DrawingImage showPanelMenuImage;
        private readonly DrawingImage exitMenuImage;

        public TrayIconManager(Window window)
        {
            dashboardWindow = window;

            trayIcon = LoadResourceIcon(
                "Assets/Icons/PausaVital.ico",
                 SystemIcons.Application);

            statisticsIcon = LoadResourceIcon(
                "Assets/Icons/statistics.ico",
                SystemIcons.Information);

            showPanelIcon = LoadResourceIcon(
                "Assets/Icons/ShowPanel.ico",
                 SystemIcons.Application);

            exitIcon = LoadResourceIcon(
                "Assets/Icons/Exit.ico",
                SystemIcons.Error);

            statisticsMenuImage = statisticsIcon.ToBitmap();
            showPanelMenuImage = showPanelIcon.ToBitmap();
            statisticsMenuImage = statisticsIcon.ToBitmap();
            exitMenuImage = exitIcon.ToBitmap();

            contextMenu = new ContextMenuStrip();

            contextMenu.ImageScalingSize =
                new System.Drawing.Size(18, 18);

            var showMenuItem = new ToolStripMenuItem
            {
                Text = "Mostrar panel",
                Image = showPanelMenuImage,
                ImageScaling = ToolStripItemImageScaling.SizeToFit
            };
            showMenuItem.Font = new Font(
                showMenuItem.Font,
                WinDrawingFontStyle.Bold);
            showMenuItem.Click += (_, _) => ShowDashboard();

            var statsMenuItem = new ToolStripMenuItem
            {
                Text = "Estadísticas avanzadas",
                Image = statisticsMenuImage,
                ImageScaling =
                        ToolStripItemImageScaling.SizeToFit
            };
            statsMenuItem.Click += (_, _) => ShowStatisticsWindow();

            var exitMenuItem = new ToolStripMenuItem
            {
                Text = "Salir",
                Image = exitMenuImage,
                ImageScaling = ToolStripItemImageScaling.SizeToFit
            };
            exitMenuItem.Click += (_, _) => ExitApplication();

            contextMenu.Items.Add(showMenuItem);
            contextMenu.Items.Add(statsMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitMenuItem);

            systemTrayIcon = new NotifyIcon
            {
                Icon = trayIcon,
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

        // ICONOS
        private static DrawingIcon LoadResourceIcon(
              string resourcePath,
              DrawingIcon fallbackIcon)
        {
            try
            {
                var resourceUri = new Uri(
                    $"pack://application:,,,/{resourcePath}",
                    UriKind.Absolute);

                var resource = System.Windows.Application
                    .GetResourceStream(resourceUri);

                if (resource?.Stream is null)
                {
                    return (DrawingIcon)fallbackIcon.Clone();
                }

                using var temporaryIcon =
                    new DrawingIcon(resource.Stream);

                return (DrawingIcon)temporaryIcon.Clone();
            }
            catch
            {
                return (DrawingIcon)fallbackIcon.Clone();
            }
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

            systemTrayIcon.Dispose();
            contextMenu.Dispose();

            showPanelMenuImage.Dispose();
            statisticsMenuImage.Dispose();
            exitMenuImage.Dispose();

            showPanelIcon.Dispose();
            statisticsIcon.Dispose();
            exitIcon.Dispose();
            trayIcon.Dispose();
        }
    }
}
