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
        List<int> _avataresUsados = new List<int>();
        private Action _onMenuClick;
        private bool _isProcessingMenuClick = false;

        public Score(List<(string nombre, int puntos, int avatar)> jugadores = null, Action onMenuClick = null)
        {
            InitializeComponent();
            _onMenuClick = onMenuClick;

            Loaded += (s, e) =>
            {
                if (jugadores != null && jugadores.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[Score] Mostrando {jugadores.Count} jugadores");
                    var jugadoresOrdenados = jugadores.OrderByDescending(j => j.puntos).ToList();
                    for (int i = 0; i < Math.Min(4, jugadoresOrdenados.Count); i++)
                    {
                        lugares(i, jugadoresOrdenados[i].nombre, jugadoresOrdenados[i].puntos);
                        ImageBrush pincelListo = ObtenerPincelAvatar(jugadoresOrdenados[i].avatar);
                        var avatar = (System.Windows.Shapes.Shape)this.FindName($"Avatar{i + 1}");
                        if (avatar != null)
                            avatar.Fill = pincelListo;
                        _avataresUsados.Add(jugadoresOrdenados[i].avatar);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[Score] No hay jugadores, mostrando datos simulados");
                    _avataresUsados.Add(3);
                    _avataresUsados.Add(2);
                    _avataresUsados.Add(7);
                    _avataresUsados.Add(5);

                    string[] _nombresSimulados = { "", "Ciber_Dev", "SQL_Master", "MichiLover", "Pro_Gamer" };
                    for (int i = 0; i < 4; i++)
                    {
                        lugares(i, _nombresSimulados[i], 100 - (i * 10));
                        ImageBrush pincelListo = ObtenerPincelAvatar(_avataresUsados[i]);
                        var avatar = (System.Windows.Shapes.Shape)this.FindName($"Avatar{i + 1}");
                        if (avatar != null)
                            avatar.Fill = pincelListo;
                    }
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
            // Prevenir múltiples clicks mientras se procesa
            if (_isProcessingMenuClick)
            {
                System.Diagnostics.Debug.WriteLine("[BtnMenu_Click] Ya se está procesando un click, ignorando...");
                return;
            }

            _isProcessingMenuClick = true;
            BtnMenu.IsEnabled = false;

            System.Diagnostics.Debug.WriteLine("[BtnMenu_Click] Click en botón Menu detectado");

            try
            {
                // Permitir que la UI se actualice visualmente
                await Task.Delay(100);

                // Llamar al callback que maneja la desconexión
                // En mainControlViewModel, este método dispara IrAMenuAsync sin bloquear
                _onMenuClick?.Invoke();

                System.Diagnostics.Debug.WriteLine("[BtnMenu_Click] Callback de Menu invocado");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BtnMenu_Click] Error al hacer click en menu: {ex.Message}");
                // Permitir que el usuario intente de nuevo si algo falla
                _isProcessingMenuClick = false;
                BtnMenu.IsEnabled = true;
            }
        }
    }
}