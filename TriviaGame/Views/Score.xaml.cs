using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TriviaGame.Views
{
    /// <summary>
    /// Interaction logic for Score.xaml
    /// </summary>
    public partial class Score : Window
    {
        List<int> _avataresUsados = new List<int>();
        string[] _nombresSimulados = { "", "Ciber_Dev", "SQL_Master", "MichiLover", "Pro_Gamer" };
        public Score()
        {
            InitializeComponent();
            _avataresUsados.Add(3);
            _avataresUsados.Add(2);
            _avataresUsados.Add(7);
            _avataresUsados.Add(5);

            for (int i = 0; i < 4; i++)
            {
                lugares(i, _nombresSimulados[i], 100 - (i * 10));
                ImageBrush pincelListo = ObtenerPincelAvatar(_avataresUsados[i]);
                var avatar = (System.Windows.Shapes.Shape)this.FindName($"Avatar{i+1}");
                avatar.Fill = pincelListo;
            }

        }

        public void lugares(int lugar, string nombre, int puntos)
        {
            switch (lugar)
            {
                case 1:
                    Player1.Visibility = Visibility.Visible;
                    TxtUsuario1.Text = nombre;
                    Puntuacion1.Text = puntos.ToString();
                    break;
                case 2:
                    Player2.Visibility = Visibility.Visible;
                    TxtUsuario2.Text = nombre;
                    Puntuacion2.Text = puntos.ToString();
                    break;
                case 3:
                    Player3.Visibility = Visibility.Visible;
                    TxtUsuario3.Text = nombre;
                    Puntuacion3.Text = puntos.ToString();
                    break;
                case 4:
                    Player4.Visibility = Visibility.Visible;
                    TxtUsuario4.Text = nombre;
                    Puntuacion4.Text = puntos.ToString();
                    break;
            }
        }

        private ImageBrush ObtenerPincelAvatar(int numeroImagen)
        {
            ImageBrush pincel = new ImageBrush();
            pincel.Stretch = Stretch.UniformToFill;

            try
            {
                // Usamos el símbolo @ para poder usar las diagonales invertidas \ de Windows sin problemas
                string rutaFisica = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views", "Avatar", $"{numeroImagen}.png");

                // Obligamos a WPF a construir la imagen de forma estricta
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(rutaFisica, UriKind.Absolute);

                // Esta propiedad ignora si el archivo está "bloqueado" por otra carpeta
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                pincel.ImageSource = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fallo al cargar la ruta física: {ex.Message}");
            }

            return pincel;
        }
    }
}
