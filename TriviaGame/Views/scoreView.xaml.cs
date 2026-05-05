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
using TriviaGame.ViewModels;

namespace TriviaGame.Views
{
    /// <summary>
    /// Lógica de interacción para scoreView.xaml
    /// </summary>
    public partial class scoreView : UserControl
    {
        public scoreView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
        }

        /*private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window ventanaPrincipal = Window.GetWindow(this);
            if (ventanaPrincipal?.DataContext is mainControlViewModel vm)
            {
                vm.IrAMenu();
            }
        }*/
    }
}
