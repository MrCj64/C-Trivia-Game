using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGame.ViewModels
{
    //Clase para manejar la vista de las diferentes pantallas (menu, juego, score)
    //Esta clase es la que se va a conectar en el mainWindow.xaml.cs
    internal class mainControlViewModel : propertiesChangesViewModel
    {
        public RelayCommand mainMenuCommand { get; set; }
        public RelayCommand gameCommand { get; set; }
        public RelayCommand scoreCommand { get; set; }

        private object _currentView;
        public object CurrentView {
            get => _currentView;
            set {
                _currentView = value;
                onPropertyChanged(nameof(CurrentView));
            }
        }

        public mainControlViewModel()
        { 
            CurrentView = new mainMenuViewModel(); 

            mainMenuCommand = new RelayCommand(() => CurrentView = new mainMenuViewModel());
            gameCommand = new RelayCommand(() => CurrentView = new inGameViewModel());
            scoreCommand = new RelayCommand(() => CurrentView = new finalScoreViewModel());  
        }

    }
}
