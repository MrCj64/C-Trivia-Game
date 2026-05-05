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
            Loaded += GameView_Loaded;
            Unloaded += GameView_Unloaded;
            DataContextChanged += GameView_DataContextChanged;
        }

        private void GameView_Loaded(object sender, RoutedEventArgs e)
        {
            IniciarTemporizador();
        }

        private void GameView_Unloaded(object sender, RoutedEventArgs e)
        {
            DetenerTemporizador();
        }

        private void GameView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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
