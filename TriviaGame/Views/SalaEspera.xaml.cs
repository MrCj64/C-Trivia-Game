using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TriviaGame.Views
{
    public partial class SalaEspera : Window
    {
        DispatcherTimer _timer;
        int _tiempoRestante = 20; // Empezamos en 20 segundos
        int _jugadoresConectados = 0;
        Random _random = new Random();
        List<int> _avataresUsados = new List<int>();

        // Nombres simulados para los jugadores que van entrando
        string[] _nombresSimulados = { "", "Ciber_Dev", "SQL_Master", "MichiLover", "Pro_Gamer" };

        public SalaEspera()
        {
            InitializeComponent();
            Player1.Visibility = Visibility.Hidden;
            Player2.Visibility = Visibility.Hidden;
            Player3.Visibility = Visibility.Hidden;
            Player4.Visibility = Visibility.Hidden;
            //IniciarTemporizador();

        }

        private void IniciarTemporizador()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_tiempoRestante > 0)
            {
                if (_tiempoRestante == 15 || _tiempoRestante == 10 || _tiempoRestante == 5 || _tiempoRestante == 1)
                {
                    ConectarSiguienteJugador();
                    MessageBox.Show("¡Un nuevo jugador se ha unido a la sala!");
                }

                _tiempoRestante--;
                Timer.Content = _tiempoRestante.ToString();
            }
            else
            {
                _timer.Stop();

            }
        }

        private void ConectarSiguienteJugador()
        {
            _jugadoresConectados++;

            if (_jugadoresConectados > 4) return;

            int imagenAleatoria;

            // El ciclo do-while genera un número y verifica si la lista ya lo contiene.
            // Si ya lo tiene, repite el ciclo. Si es nuevo, sale del ciclo.
            do
            {
                imagenAleatoria = _random.Next(1, 8);
            }
            while (_avataresUsados.Contains(imagenAleatoria));

            // Agregamos el número nuevo a la lista para que no vuelva a salir
            _avataresUsados.Add(imagenAleatoria);

            string nombreAsignado = _nombresSimulados[_jugadoresConectados];

            ImageBrush pincelListo = ObtenerPincelAvatar(imagenAleatoria);
            switch (_jugadoresConectados)
            {
                case 1:
                    Avatar1.Fill = pincelListo;
                    TxtUsuario1.Text = nombreAsignado;
                    Player1.Visibility = Visibility.Visible;
                    break;
                case 2:
                    Avatar2.Fill = pincelListo;
                    TxtUsuario2.Text = nombreAsignado;
                    Player2.Visibility = Visibility.Visible;
                    break;
                case 3:
                    Avatar3.Fill = pincelListo;
                    TxtUsuario3.Text = nombreAsignado;
                    Player3.Visibility = Visibility.Visible;
                    break;
                case 4:
                    Avatar4.Fill = pincelListo;
                    TxtUsuario4.Text = nombreAsignado;
                    Player4.Visibility = Visibility.Visible;
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
                string rutaFisica = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Views", "Avatar", $"{numeroImagen}.png");

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