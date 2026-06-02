using System;
using Microsoft.Win32;

namespace PausaVital.Services
{
    public static class StartupManager
    {
        private const string AppName = "PausaVital";

        public static void EnsureAutoStart()
        {
            try
            {
                string? exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath)) return;

                string runValue = $"\"{exePath}\" --background";

                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (key != null)
                {
                    string? currentValue = key.GetValue(AppName) as string;

                    if (currentValue != runValue)
                    {
                        key.SetValue(AppName, runValue);
                    }
                }
            }
            catch
            {
            }
        }
    }
}