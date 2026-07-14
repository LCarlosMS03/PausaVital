using System.Windows;
using PausaVital.Services;

namespace PausaVital.Views
{
    public partial class ClosePromptWindow : Window
    {
        public string SelectedAction { get; private set; } = "";

        public ClosePromptWindow()
        {
            InitializeComponent();
            ApplyTranslations();
        }

        private void ApplyTranslations()
        {
            Title = TranslationManager.Get("ClosePromptWindowTitle", "Exit Pausa Vital");

            TitleText.Text = TranslationManager.Get(
                "ClosePromptTitle",
                "How do you want to close the app?");

            DescriptionText.Text = TranslationManager.Get(
                "ClosePromptDescription",
                "You can keep Pausa Vital running in the background to maintain your streaks, or exit completely.");

            RememberCheckBox.Content = TranslationManager.Get(
                "ClosePromptRemember",
                "Remember my choice and don't ask again");

            MinimizeButton.Content = TranslationManager.Get(
                "ClosePromptMinimize",
                "Minimize to Tray");

            ExitButton.Content = TranslationManager.Get(
                "ClosePromptExit",
                "Exit Completely");
        }

        private void OnMinimizeClicked(object sender, RoutedEventArgs e)
        {
            SelectedAction = "Minimize";

            if (RememberCheckBox.IsChecked == true)
            {
                var prefs = PreferencesManager.Load();
                prefs.CloseAction = "Minimize";
                PreferencesManager.Save(prefs);
            }

            DialogResult = true;
        }

        private void OnExitClicked(object sender, RoutedEventArgs e)
        {
            SelectedAction = "Exit";

            if (RememberCheckBox.IsChecked == true)
            {
                var prefs = PreferencesManager.Load();
                prefs.CloseAction = "Exit";
                PreferencesManager.Save(prefs);
            }

            DialogResult = true;
        }
    }
}