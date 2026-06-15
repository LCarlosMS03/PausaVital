using System;
using System.IO;
using System.Text.Json;

namespace PausaVital.Services
{
    public class AppPreferences
    {
        public string CloseAction { get; set; } = "Ask";
        public string SelectedMode { get; set; } = "None";
        public string Language { get; set; } = "es"; // Idioma español
    }

    public static class PreferencesManager
    {
        private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "user_preferences.json");

        public static AppPreferences Load()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<AppPreferences>(json) ?? new AppPreferences();
                }
                catch { }
            }
            return new AppPreferences();
        }

        public static void Save(AppPreferences prefs)
        {
            try
            {
                string json = JsonSerializer.Serialize(prefs);
                File.WriteAllText(FilePath, json);
            }
            catch { }
        }
    }
}