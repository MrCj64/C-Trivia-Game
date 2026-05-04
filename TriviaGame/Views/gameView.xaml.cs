using System;
using System.Collections.Generic;
using System.Text;
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
    /// Lógica de interacción para gameView.xaml
    /// </summary>
    public partial class gameView : UserControl
    {
        DispatcherTimer _timer;
        int _tiempoRestante = 10;
        SalaEspera ventana = new SalaEspera();  
        public gameView()
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
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
