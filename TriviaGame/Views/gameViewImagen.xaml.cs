using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TriviaGame.Views
{
    public partial class gameViewImagen : UserControl
    {
        DispatcherTimer _timer;
        int _tiempoRestante = 10;

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
                if (Timer != null)
                    Timer.Content = _tiempoRestante.ToString();
            }
            else
            {
                _timer.Stop();
            }
        }
    }
}