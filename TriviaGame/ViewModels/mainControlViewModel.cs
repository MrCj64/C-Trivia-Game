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
        private int puntuacionTotal = 0;
        private string categoriaActual = "";

        public string jugadorActual = "";
        public int idJugadorActual = -1;

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
                startGame: async (categoryId) => await selectedCategory(categoryId), 
                scoreMenu: () => CurrentView = new finalScoreViewModel(IrAMenu)
            );
        }

        public async Task selectedCategory(string categoryId)
        {

            if (!int.TryParse(categoryId, out int idCategoria))
            {
                return;
            }

            await AbrirSalaEspera(idCategoria);
        }

        private async Task AbrirSalaEspera(int idCategoria)
        {

            var salaEspera = new SalaEspera(
                idCategoria.ToString(), 
                jugadorActual, 
                async () =>
                {
                    await IniciarJuego(idCategoria);
                }
            );

            CurrentView = salaEspera;
        }

        private async Task IniciarJuego(int idCategoria)
        {

            aciertosTotal = 0;
            erroresTotal = 0;
            puntuacionTotal = 0;

            categoriaActual = await queryService.GetNombreCategoria(idCategoria);

            var preguntasRaw = await queryService.GetPreguntas(idCategoria);

            if (preguntasRaw == null || preguntasRaw.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[IniciarJuego] ERROR - No hay preguntas para categoría {idCategoria}");
                return;
            }


            var preguntas = new List<questionModel>();
            foreach (var p in preguntasRaw.OrderBy(_ => random.Next()))
            {
                var respuestasRaw = await queryService.GetRespuestas(Convert.ToInt32(p["idPregunta"]));
                if (respuestasRaw == null) continue;

                var respuestas = respuestasRaw
                    .OrderBy(_ => random.Next())
                    .Select(r => new answerModel
                    {
                        answer = r["textRespuesta"]?.ToString() ?? "",
                        isCorrect = Convert.ToBoolean(r["EsCorrecta"]),
                        mediaPath = r["rutaRespuesta"]?.ToString() ?? "",
                        answerType = r["tipoRespuesta"]?.ToString() ?? "TEXT"
                    }).ToList();

                string tipo = respuestas.FirstOrDefault()?.answerType?.ToUpper() ?? "TEXT";

                questionModel pregunta = tipo switch
                {
                    "SOUND" => new audioQuestionModel { pathAudio = respuestas.FirstOrDefault()?.mediaPath },
                    "IMG" => new imageQuestionModel { pathImage = respuestas.FirstOrDefault()?.mediaPath },
                    _ => new textQuestionModel()
                };

                pregunta.question = p["nomPregunta"].ToString();
                pregunta.categoryId = idCategoria;
                pregunta.points = Convert.ToInt32(p["puntuacionPregunta"]);
                pregunta.answers = respuestas;
                preguntas.Add(pregunta);
            }

            MostrarPregunta(preguntas, 0);
        }

        private void MostrarPregunta(List<questionModel> preguntas, int index)
        {
            if (index >= preguntas.Count)
            {
                if (idJugadorActual != -1)
                    queryService.insertaPuntuacion(idJugadorActual, preguntas[0].categoryId, puntuacionTotal);

                CurrentView = new Score();
                return;
            }

            Action siguiente = () =>
            {
                switch (CurrentView)
                {
                    case textGameViewModel vm: 
                        aciertosTotal += vm.Aciertos; 
                        erroresTotal += vm.Errores;
                        puntuacionTotal += vm.Puntuacion;
                        break;
                    case audioGameViewModel vm: 
                        aciertosTotal += vm.Aciertos; 
                        erroresTotal += vm.Errores;
                        puntuacionTotal += vm.Puntuacion;
                        break;
                    case ImageGameViewModel vm: 
                        aciertosTotal += vm.Aciertos; 
                        erroresTotal += vm.Errores;
                        puntuacionTotal += vm.Puntuacion;
                        break;
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
                startGame: async (categoryId) => await selectedCategory(categoryId),
                scoreMenu: () => CurrentView = new finalScoreViewModel(IrAMenu)
            );
        }
    }
}
