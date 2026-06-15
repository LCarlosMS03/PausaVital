using System.Collections.Generic;

namespace PausaVital.Services
{
    public static class TranslationManager
    {
        public static string CurrentLanguage = "es";

        private static readonly Dictionary<string, string> Es = new()
        {
            { "Starting", "Iniciando conexión..." },
            { "Connected", "Conectado" },
            { "Disconnected", "Desconectado. Reintentando..." },
            { "Streak", "🔥 Racha:" },
            { "Shields", "🛡️ Escudos:" },
            { "Resting", "DESCANSANDO... ¡NO SE MUEVA!" },
            { "Saved", "¡SALVADO POR ESCUDO!" },
            { "Broken", "¡RACHA TERMINADA!" },
            { "Work", "Trabajo" },
            { "Break", "Descanso" },
            { "RuleTip", "¡Mire a 6 metros por 20 segundos! No toque el ratón ni teclado." },
            { "PomodoroTip", "¡Hora de un descanso de 5 minutos! Levántese y estírese." },
            { "Hydration", "¡Hora de tomar un vaso con agua para mantener la salud y el enfoque!" },
            { "HideToBackgroundBtn", "Minimizar al Fondo" },
            { "RunningBackground", "Ejecutándose en segundo plano. Clic derecho en el ícono para salir." },
            { "MenuShow", "Mostrar Panel" },
            { "MenuStats", "Estadísticas Avanzadas" },
            { "MenuExit", "Salir" },
            { "StatsTitle", "Resumen de Rendimiento" },
            { "SuccessRateLabel", "TASA DE ÉXITO" },
            { "CompletedLabel", "COMPLETADO" },
            { "FailedLabel", "FALLADO" },
            { "LiveIdleLabel", "Inactividad del Sistema: " },
            { "CloseDashboardBtn", "Cerrar Panel" }
        };

        public static void Initialize()
        {
            var prefs = PreferencesManager.Load();
            CurrentLanguage = prefs.Language;
        }

        public static string Get(string key, string defaultEn)
        {
            if (CurrentLanguage == "es" && Es.ContainsKey(key))
            {
                return Es[key];
            }
            return defaultEn;
        }
    }
}