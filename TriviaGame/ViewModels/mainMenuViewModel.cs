using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGame.ViewModels
{
    //Clase para implementar la logica del menu de inicio
    public class mainMenuViewModel : propertiesChangesViewModel
    {
        public RelayCommand startGameCommand { get; set; }
        public RelayCommand scoreMenuCommand { get; set; }

        public mainMenuViewModel(Func<String, Task> startGame, Action scoreMenu)
        {
            startGameCommand = new RelayCommand(async (obj) =>
            {
                string category = obj as string;
                await startGame(category);
            });

            scoreMenuCommand = new RelayCommand(scoreMenu);
        }

    }
}
