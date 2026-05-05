using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TriviaGame.Views
{
    public partial class gameViewSonido : UserControl
    {
        DispatcherTimer _timer;
        int _tiempoRestante = 10;

        public gameViewSonido()
        {
            InitializeComponent();
            Loaded += GameViewSonido_Loaded;
            Unloaded += GameViewSonido_Unloaded;
            DataContextChanged += GameViewSonido_DataContextChanged;
        }

        private void GameViewSonido_Loaded(object sender, RoutedEventArgs e)
        {
            IniciarTemporizador();
        }

        private void GameViewSonido_Unloaded(object sender, RoutedEventArgs e)
        {
            DetenerTemporizador();
        }

        private void GameViewSonido_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsLoaded)
            {
                IniciarTemporizador();
            }
        }

        private void IniciarTemporizador()
        {
            DetenerTemporizador();
            _tiempoRestante = 10;
            if (Timer != null)
                Timer.Content = _tiempoRestante.ToString();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void DetenerTemporizador()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                _timer = null;
            }
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
                if (DataContext is TriviaGame.ViewModels.IGameQuestion gameQuestion)
                {
                    gameQuestion.RevealAnswer();
                }
            }
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            // No detener el temporizador aquí para que siga corriendo tras la selección.
        }
    }
}