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
        private object currentView;
        private queryService queryService;
        private Random random;

        private int aciertosTotal = 0;
        private int erroresTotal = 0;
        private string categoriaActual = "";


        public object CurrentView
        {
            get => currentView;
            set
            {
                currentView = value;
                onPropertyChanged(nameof(CurrentView));
            }
        }

        public mainControlViewModel()
        {
            queryService = new queryService();
            random = new Random();
            CurrentView = new loginViewModel(
                this,
                startGame: (categoryId) => selectedCategory(categoryId),
                scoreMenu: () => CurrentView = new finalScoreViewModel(IrAMenu, categoriaActual, aciertosTotal, erroresTotal)
            );

        }

        public void selectedCategory(string categoryId)
        {
            aciertosTotal = 0;
            erroresTotal = 0;

            if (!int.TryParse(categoryId, out int idCategoria)) return;

            categoriaActual = queryService.GetNombreCategoria(idCategoria);
            var preguntas = queryService.GetPreguntas(idCategoria);
            if (preguntas.Count == 0) return;

            var preguntasAleatorias = preguntas.OrderBy(x => random.Next()).ToList();
            foreach (var pregunta in preguntasAleatorias)
            {
                int idPregunta = (int)pregunta["idPregunta"];
                var respuestas = queryService.GetRespuestas(idPregunta);
                pregunta["respuestas"] = respuestas.OrderBy(x => random.Next()).ToList();
            }

            MostrarPregunta(preguntasAleatorias, 0);
        }

        private void MostrarPregunta(List<Dictionary<string, object>> preguntas, int index)
        {
            if (index >= preguntas.Count)
            {
                CurrentView = new finalScoreViewModel(IrAMenu, categoriaActual, aciertosTotal, erroresTotal);
                return;
            }

            var respuestas = (List<Dictionary<string, object>>)preguntas[index]["respuestas"];
            string tipo = respuestas.Count > 0
                ? respuestas[0]["tipoRespuesta"]?.ToString() ?? "TEXT"
                : "TEXT";

            Action siguiente = () =>
            {
                switch (CurrentView)
                {
                    case textGameViewModel vm: aciertosTotal += vm.Aciertos; erroresTotal += vm.Errores; break;
                    case audioGameViewModel vm: aciertosTotal += vm.Aciertos; erroresTotal += vm.Errores; break;
                    case ImageGameViewModel vm: aciertosTotal += vm.Aciertos; erroresTotal += vm.Errores; break;
                }
                MostrarPregunta(preguntas, index + 1);
            };

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
                scoreMenu: () => CurrentView = new finalScoreViewModel(IrAMenu, categoriaActual, aciertosTotal, erroresTotal)
            );
        }
    }
}
