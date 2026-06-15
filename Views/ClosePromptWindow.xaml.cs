using System.Windows;
using PausaVital.Services;

namespace PausaVital.Views
{
    public partial class ClosePromptWindow : Window
    {
        public string SelectedAction { get; private set; } = "Cancel";

        public ClosePromptWindow()
        {
            InitializeComponent();
        }

        private void OnMinimizeClicked(object sender, RoutedEventArgs e)
        {
            SavePreference("Minimize");
            SelectedAction = "Minimize";
            DialogResult = true;
        }

        private void OnExitClicked(object sender, RoutedEventArgs e)
        {
            SavePreference("Exit");
            SelectedAction = "Exit";
            DialogResult = true;
        }

        private void SavePreference(string action)
        {
            if (RememberCheckBox.IsChecked == true)
            {
                var prefs = PreferencesManager.Load();
                prefs.CloseAction = action;
                PreferencesManager.Save(prefs);
            }
        }
    }
}