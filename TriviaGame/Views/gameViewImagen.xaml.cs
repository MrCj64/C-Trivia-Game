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
using System.Windows.Threading;

namespace TriviaGame.Views
{
    /// <summary>
    /// Lógica de interacción para gameViewImagen.xaml
    /// </summary>
    public partial class gameViewImagen : UserControl
    {
        DispatcherTimer _timer;
        int _tiempoRestante = 10;
        SalaEspera ventana = new SalaEspera();
        public gameViewImagen()
        {
            InitializeComponent();
            IniciarTemporizador();
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
                _tiempoRestante--;
                Timer.Content = _tiempoRestante.ToString();
            }
            else
            {
                _timer.Stop();
                ventana.Show();
            }
        }
    }
}
