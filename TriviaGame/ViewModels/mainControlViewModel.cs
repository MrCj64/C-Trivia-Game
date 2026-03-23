using System;
using System.Collections.Generic;
using System.Text;
using TriviaGame.Views;
using System.Linq;
using TriviaGame.Services;
using TriviaGame.Models;

namespace TriviaGame.ViewModels
{
    //Clase para manejar la vista de las diferentes pantallas (menu, juego, score)
    //Esta clase es la que se va a conectar en el mainWindow.xaml.cs
    internal class mainControlViewModel : propertiesChangesViewModel
    {
        public RelayCommand mainMenuCommand { get; set; }
        public RelayCommand gameCommand { get; set; }
        public RelayCommand scoreCommand { get; set; }

        public RelayCommand LoginCommand { get; set; }


        private object _currentView;
        private queryService _queryService;
        private Random _random;

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
            _queryService = new queryService();
            _random = new Random();
            CurrentView = new loginViewModel(
                this,
                startGame: (categoryId) => selectedCategory(categoryId),
                scoreMenu: () => CurrentView = new finalScoreViewModel()
            );

        }

        public void selectedCategory(string categoryId)
        {
            if (!int.TryParse(categoryId, out int idCategoria))
                return;

            string tipoRespuesta = _queryService.GetTipoPregunta(idCategoria);

            List<Dictionary<string, object>> preguntas = _queryService.GetPreguntas(idCategoria);

            if (preguntas.Count == 0)
            {
                return;
            }

            List<Dictionary<string, object>> preguntasAleatorias = preguntas.OrderBy(x => _random.Next()).ToList();

            foreach (var pregunta in preguntasAleatorias)
            {
                int idPregunta = (int)pregunta["idPregunta"];
                List<Dictionary<string, object>> respuestas = _queryService.GetRespuestas(idPregunta);
                pregunta["respuestas"] = respuestas.OrderBy(x => _random.Next()).ToList();
            }

            switch (tipoRespuesta.ToUpper())
            {
                case "TEXT":
                    CurrentView = new textGameViewModel(preguntasAleatorias);
                    break;
                case "SOUND":
                    CurrentView = new audioGameViewModel(preguntasAleatorias);
                    break;
                case "IMG":
                    CurrentView = new ImageGameViewModel(preguntasAleatorias);
                    break;
            }

        }
        public void IrAMenu()
        {
            CurrentView = new mainMenuViewModel(
                startGame: (categoryId) => selectedCategory(categoryId),
                scoreMenu: () => CurrentView = new finalScoreViewModel()
            );
        }
    }
}
