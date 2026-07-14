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
            { "NotReadyTitle", "No está listo" },
            { "BackendNotReady", "Espera a que el backend se conecte." },
            { "StatsTitle", "Resumen de Rendimiento" },
            { "SuccessRateLabel", "TASA DE ÉXITO" },
            { "CompletedLabel", "COMPLETADO" },
            { "FailedLabel", "FALLADO" },
            { "LiveIdleLabel", "Inactividad del Sistema: " },
            { "CloseDashboardBtn", "Cerrar Panel" },
            { "WorkTimeLabel", "TIEMPO DE TRABAJO" },
            { "Retrying", "Reintentando conexión..." },
            { "BreakTitle", "Hora de descansar" },
            { "BreakCompletedTitle", "Descanso completado" },
            { "BreakCompletedMessage", "¡Buen trabajo! Racha actualizada." },
            { "ShieldUsedTitle", "¡Escudo usado!" },
            { "ShieldUsedMessage", "Te moviste, pero un escudo salvó tu racha." },
            { "StreakBrokenTitle", "Racha terminada" },
            { "StreakBrokenMessage", "Te moviste antes de completar el descanso." },
            { "HydrationTitle", "Recordatorio de hidratación" },
            { "ClosePromptWindowTitle", "Salir de Pausa Vital" },
            { "ClosePromptTitle", "¿Cómo quieres cerrar la aplicación?" },
            { "ClosePromptDescription", "Puedes mantener Pausa Vital ejecutándose en segundo plano para conservar tus rachas, o salir completamente." },
            { "ClosePromptRemember", "Recordar mi elección y no volver a preguntar" },
            { "ClosePromptMinimize", "Minimizar a la bandeja" },
            { "ClosePromptExit", "Salir completamente" },
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