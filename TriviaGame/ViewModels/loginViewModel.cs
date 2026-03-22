using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TriviaGame.Services;
using TriviaGame.Views;

namespace TriviaGame.ViewModels
{
    internal class loginViewModel : propertiesChangesViewModel
    {
        private string username;
        private string message;
        public string password;

        private mainControlViewModel mainVM;
        private queryService queryService;

        Action<String> a;
        Action b;
        public string Username
        {
            get => username;
            set { username = value; onPropertyChanged(); }
        }

        public string Password
        {
            get => password;
            set { password = value; onPropertyChanged(); }
        }

        public string Message
        {
            get => message;
            set { message = value; onPropertyChanged(); }
        }
        public ICommand LoginCommand { get; }
        public ICommand InsertaJugador { get; }

        public loginViewModel(mainControlViewModel mainVM, Action<String> startGame, Action scoreMenu)
        {
            a = startGame;
            b = scoreMenu;
            this.mainVM = mainVM;
            queryService = new queryService();

            LoginCommand = new RelayCommand(Login);
            InsertaJugador = new RelayCommand(Insertar);
        }

        public void Login()
        {

            bool user = queryService.LoginJugador(username, password);

            if (user != false)
            {
                Message = "Login exitoso";
                mainVM.CurrentView = new mainMenuViewModel(a, b);
            }
            else
            {
                Message = "Usuario o contraseña incorrectos";
            }
        }

        public void Insertar()
        {

            bool success = queryService.insertaJugador(Username, password);

            if (success)
                Message = "Usuario registrado exitosamente";

            else
                Message = "Error al registrar";
        }
    }
}
