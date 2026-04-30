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

            var preguntasRaw = queryService.GetPreguntas(idCategoria);
            if (preguntasRaw.Count == 0) return;

            var preguntas = preguntasRaw
                .OrderBy(_ => random.Next())
                .Select(p =>
                {
                    var respuestasRaw = queryService.GetRespuestas((int)p["idPregunta"])
                        .OrderBy(_ => random.Next())
                        .Select(r => new answerModel
                        {
                            answer = r["textRespuesta"]?.ToString() ?? "",
                            isCorrect = (bool)r["EsCorrecta"],
                            mediaPath = r["rutaRespuesta"]?.ToString() ?? "",
                            answerType = r["tipoRespuesta"]?.ToString() ?? "TEXT"
                        })
                        .ToList();

                    string tipo = respuestasRaw.FirstOrDefault()?.answerType?.ToUpper() ?? "TEXT";

                    questionModel pregunta = tipo switch
                    {
                        "SOUND" => new audioQuestionModel { pathAudio = respuestasRaw.FirstOrDefault()?.mediaPath },
                        "IMG" => new imageQuestionModel { pathImage = respuestasRaw.FirstOrDefault()?.mediaPath },
                        _ => new textQuestionModel()
                    };

                    pregunta.question = p["nomPregunta"].ToString();
                    pregunta.categoryId = idCategoria;
                    pregunta.answers = respuestasRaw;

                    return pregunta;
                })
                .ToList();

            MostrarPregunta(preguntas, 0);
        }

        private void MostrarPregunta(List<questionModel> preguntas, int index)
        {
            if (index >= preguntas.Count)
            {
                CurrentView = new finalScoreViewModel(IrAMenu, categoriaActual, aciertosTotal, erroresTotal);
                return;
            }

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

            CurrentView = preguntas[index] switch
            {
                audioQuestionModel => new audioGameViewModel(new List<questionModel> { preguntas[index] }, siguiente),
                imageQuestionModel => new ImageGameViewModel(new List<questionModel> { preguntas[index] }, siguiente),
                _ => new textGameViewModel(new List<questionModel> { preguntas[index] }, siguiente)
            };
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
