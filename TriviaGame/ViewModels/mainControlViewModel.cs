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
    public class mainControlViewModel : propertiesChangesViewModel
    {
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
                scoreMenu: () => CurrentView = new finalScoreViewModel(IrAMenu)
            );

        }

        public void selectedCategory(string categoryId)
        {
            if (!int.TryParse(categoryId, out int idCategoria)) return;

            var preguntas = _queryService.GetPreguntas(idCategoria);
            if (preguntas.Count == 0) return;

            var preguntasAleatorias = preguntas.OrderBy(x => _random.Next()).ToList();
            foreach (var pregunta in preguntasAleatorias)
            {
                int idPregunta = (int)pregunta["idPregunta"];
                var respuestas = _queryService.GetRespuestas(idPregunta);
                pregunta["respuestas"] = respuestas.OrderBy(x => _random.Next()).ToList();
            }

            MostrarPregunta(preguntasAleatorias, 0);
        }

        private void MostrarPregunta(List<Dictionary<string, object>> preguntas, int index)
        {
            if (index >= preguntas.Count)
            {
                CurrentView = new finalScoreViewModel(IrAMenu);
                return;
            }

            var respuestas = (List<Dictionary<string, object>>)preguntas[index]["respuestas"];
            string tipo = respuestas.Count > 0
                ? respuestas[0]["tipoRespuesta"]?.ToString() ?? "TEXT"
                : "TEXT";

            // Callback para avanzar a la siguiente pregunta
            Action siguiente = () => MostrarPregunta(preguntas, index + 1);

            switch (tipo.ToUpper())
            {
                case "TEXT":
                    CurrentView = new textGameViewModel(
                        new List<Dictionary<string, object>> { preguntas[index] }, siguiente);
                    break;
                case "SOUND":
                    CurrentView = new audioGameViewModel(
                        new List<Dictionary<string, object>> { preguntas[index] }, siguiente);
                    break;
                case "IMG":
                    CurrentView = new ImageGameViewModel(
                        new List<Dictionary<string, object>> { preguntas[index] }, siguiente);
                    break;
                default:
                    CurrentView = new textGameViewModel(
                        new List<Dictionary<string, object>> { preguntas[index] }, siguiente);
                    break;
            }
        }
        public void IrAMenu()
        {
            CurrentView = new mainMenuViewModel(
                startGame: (categoryId) => selectedCategory(categoryId),
                scoreMenu: () => CurrentView = new finalScoreViewModel(IrAMenu)
            );
        }
    }
}
