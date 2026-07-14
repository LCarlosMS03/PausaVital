using System.ComponentModel;
using System.Windows;
using PausaVital.Services;

namespace PausaVital.Views
{
    public partial class ModeSelectionWindow : Window
    {
        private bool modeSelected;

        public ModeSelectionWindow()
        {
            InitializeComponent();

            Closing += OnWindowClosing;
        }

        private void OnRule20Clicked(
            object sender,
            RoutedEventArgs e)
        {
            SaveModeAndClose("20-20-20");
        }

        private void OnPomodoroClicked(
            object sender,
            RoutedEventArgs e)
        {
            SaveModeAndClose("Pomodoro");
        }

        private void SaveModeAndClose(string mode)
        {
            var preferences = PreferencesManager.Load();

            preferences.SelectedMode = mode;

            PreferencesManager.Save(preferences);

            modeSelected = true;
            DialogResult = true;
        }

        private void OnWindowClosing(
            object? sender,
            CancelEventArgs e)
        {
            if (modeSelected)
            {
                return;
            }

            System.Windows.MessageBox.Show(
                "Debes seleccionar un modo para continuar.",
                "Selección requerida",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            e.Cancel = true;
        }
    }
}