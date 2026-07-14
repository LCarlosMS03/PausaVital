using System.Windows;
using PausaVital.Services;

namespace PausaVital.Views
{
    public partial class ClosePromptWindow : Window
    {
        public string SelectedAction { get; private set; } = string.Empty;

        public ClosePromptWindow()
        {
            InitializeComponent();
        }

        private void OnMinimizeClicked(object sender, RoutedEventArgs e)
        {
            SelectedAction = "Minimize";

            SaveClosePreferenceIfRequested("Minimize");

            DialogResult = true;
        }

        private void OnExitClicked(object sender, RoutedEventArgs e)
        {
            SelectedAction = "Exit";

            SaveClosePreferenceIfRequested("Exit");

            DialogResult = true;
        }

        private void SaveClosePreferenceIfRequested(string action)
        {
            if (RememberCheckBox.IsChecked != true)
            {
                return;
            }

            var preferences = PreferencesManager.Load();
            preferences.CloseAction = action;
            PreferencesManager.Save(preferences);
        }
    }
}