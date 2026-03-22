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

namespace TriviaGame.Views
{
    /// <summary>
    /// Lógica de interacción para mainMenuView.xaml
    /// </summary>
    public partial class mainMenuView : UserControl
    {
        public mainMenuView()
        {
            InitializeComponent();
        }

        /*private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            gameView game = new gameView();
            game.Focus();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            gameView vistaJuego = new gameView();

            Window ventanaPrincipal = Window.GetWindow(this);
            if (ventanaPrincipal != null)
            {
                ventanaPrincipal.Content = vistaJuego;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            gameViewImagen vistaJuego = new gameViewImagen();

            Window ventanaPrincipal = Window.GetWindow(this);
            if (ventanaPrincipal != null)
            {
                ventanaPrincipal.Content = vistaJuego;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            gameViewSonido vistaJuego = new gameViewSonido();

            Window ventanaPrincipal = Window.GetWindow(this);
            if (ventanaPrincipal != null)
            {
                ventanaPrincipal.Content = vistaJuego;
            }
        }

        private void BtnArchivo_Click(object sender, RoutedEventArgs e)
        {
            scoreView vistaJuego = new scoreView();

            Window ventanaPrincipal = Window.GetWindow(this);
            if (ventanaPrincipal != null)
            {
                ventanaPrincipal.Content = vistaJuego;
            }
        }*/
    }
}
