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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TriviaGame.Views
{
    public partial class Score : UserControl
    {
        private Action _onMenuClick;
        private bool _isProcessingMenuClick = false;

        public Score(List<(string nombre, int puntos, int avatar)> jugadores = null, Action onMenuClick = null)
        {
            InitializeComponent();
            _onMenuClick = onMenuClick;

            Loaded += (s, e) =>
            {
                Player1.Visibility = Visibility.Collapsed;
                Player2.Visibility = Visibility.Collapsed;
                Player3.Visibility = Visibility.Collapsed;

                if (jugadores != null && jugadores.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[Score] Mostrando {jugadores.Count} jugadores reales");
                    var top3 = jugadores
                        .OrderByDescending(j => j.puntos)
                        .Take(3)
                        .ToList();
                    for (int i = 0; i < top3.Count; i++)
                    {
                        lugares(i, top3[i].nombre, top3[i].puntos);

                        ImageBrush pincel = ObtenerPincelAvatar(top3[i].avatar);
                        var avatarShape = (System.Windows.Shapes.Shape)this.FindName($"Avatar{i + 1}");
                        if (avatarShape != null)
                            avatarShape.Fill = pincel;
                    }
                }
                else
                {
                    // Si llegamos aquí sin datos, algo salió mal — loguear para debug
                    System.Diagnostics.Debug.WriteLine("[Score] ADVERTENCIA: jugadores es null o vacío. Revisar OnGameOverReceived.");
                }
            };
        }

        public void lugares(int lugar, string nombre, int puntos)
        {
            switch (lugar)
            {
                case 0:
                    Player1.Visibility = Visibility.Visible;
                    TxtUsuario1.Text = nombre;
                    Puntuacion1.Text = puntos.ToString();
                    break;
                case 1:
                    Player2.Visibility = Visibility.Visible;
                    TxtUsuario2.Text = nombre;
                    Puntuacion2.Text = puntos.ToString();
                    break;
                case 2:
                    Player3.Visibility = Visibility.Visible;
                    TxtUsuario3.Text = nombre;
                    Puntuacion3.Text = puntos.ToString();
                    break;
                case 3:
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
                string rutaFisica = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views", "avatar", $"avatar{numeroImagen}.png");

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(rutaFisica, UriKind.Absolute);

                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                pincel.ImageSource = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ObtenerPincelAvatar] Fallo al cargar avatar: {ex.Message}");
            }

            return pincel;
        }

        /// <summary>
        /// Manejador de click del botón Menu
        /// Se ejecuta de forma asincrónica para permitir que la UI se actualice
        /// </summary>
        private async void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_isProcessingMenuClick) return;

            _isProcessingMenuClick = true;
            BtnMenu.IsEnabled = false;

            try
            {
                await Task.Delay(100);
                _onMenuClick?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BtnMenu_Click] Error: {ex.Message}");
                _isProcessingMenuClick = false;
                BtnMenu.IsEnabled = true;
            }
        }
    }
}