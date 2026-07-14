using System.Windows;
using PausaVital.Services;

namespace PausaVital.Views
{
    public partial class ModeSelectionWindow : Window
    {
        public ModeSelectionWindow()
        {
            InitializeComponent();
        }

        private void OnRule20Clicked(object sender, RoutedEventArgs e)
        {
            SaveAndClose("20-20-20");
        }

        private void OnPomodoroClicked(object sender, RoutedEventArgs e)
        {
            SaveAndClose("Pomodoro");
        }

        private void SaveAndClose(string mode)
        {
            var prefs = PreferencesManager.Load();
            prefs.SelectedMode = mode;
            PreferencesManager.Save(prefs);
            Close();
        }
    }
}