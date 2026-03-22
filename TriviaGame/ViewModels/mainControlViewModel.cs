using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGame.ViewModels
{
    //Clase para manejar la vista de las diferentes pantallas (menu, juego, score)
    //Esta clase es la que se va a conectar en el mainWindow.xaml.cs
    internal class mainControlViewModel : propertiesChangesViewModel
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                onPropertyChanged(nameof(CurrentView));
            }
        }

        public mainControlViewModel()
        {
            CurrentView = new mainMenuViewModel(
                startGame: (category) => selectedCategory(category),
                scoreMenu: () => CurrentView = new finalScoreViewModel()
            );
        }

        public void selectedCategory(string category)
        {
            
        }
    }
}
