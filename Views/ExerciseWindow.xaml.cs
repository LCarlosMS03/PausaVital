using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;

namespace PausaVital.Views
{
    public partial class ExerciseWindow : Window
    {
        public ExerciseWindow()
        {
            InitializeComponent();

            if (System.Windows.Application.Current.MainWindow != null)
            {
                Left = System.Windows.Application.Current.MainWindow.Left +
                       System.Windows.Application.Current.MainWindow.Width + 10;

                Top = System.Windows.Application.Current.MainWindow.Top;
            }

            LoadRandomExercise();
        }

        private void LoadRandomExercise()
        {
            Random random = new Random();

            int exercise = random.Next(6);

            switch (exercise)
            {
                case 0:
                    ExerciseTitle.Text = "Estiramiento de cuello";

                    ExerciseDescription.Text =
                        "Inclina suavemente tu cabeza hacia ambos lados durante 10 segundos.";

                    ExerciseImage.Source =
                        new BitmapImage(
                            new Uri(
                                "pack://application:,,,/Assets/Exercises/cuello.png"));

                    break;

                case 1:
                    ExerciseTitle.Text = "Estiramiento de espalda";

                    ExerciseDescription.Text =
                        "Extiende los brazos hacia tu cabeza y arquea ligeramente la espalda.";

                    ExerciseImage.Source =
                        new BitmapImage(
                            new Uri(
                                "pack://application:,,,/Assets/Exercises/espalda.png"));

                    break;

                case 2:
                    ExerciseTitle.Text = "Relajación de hombros";

                    ExerciseDescription.Text =
                        "Realiza movimientos circulares con los hombros.";

                    ExerciseImage.Source =
                        new BitmapImage(
                            new Uri(
                                "pack://application:,,,/Assets/Exercises/hombros.png"));

                    break;

                case 3:
                    ExerciseTitle.Text = "Relajación de pecho";

                    ExerciseDescription.Text =
                        "Extiende tus brazos hacia el respaldo de la silla.";

                    ExerciseImage.Source =
                        new BitmapImage(
                            new Uri(
                                "pack://application:,,,/Assets/Exercises/pecho.png"));

                    break;

                case 4:
                    ExerciseTitle.Text = "Relajación de costado";

                    ExerciseDescription.Text =
                        "Extiende tu cuerpo hacia un lado y mantén la posición durante 10 segundos.";

                    ExerciseImage.Source =
                        new BitmapImage(
                            new Uri(
                                "pack://application:,,,/Assets/Exercises/costado.png"));

                    break;

                case 5:
                    ExerciseTitle.Text = "Movimiento de muñecas";

                    ExerciseDescription.Text =
                        "Realiza un movimientos circulares con las muñecas durante 15 segundos.";

                    ExerciseImage.Source =
                        new BitmapImage(
                            new Uri(
                                "pack://application:,,,/Assets/Exercises/munecas.png"));

                    break;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
