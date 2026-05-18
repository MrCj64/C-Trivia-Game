using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TriviaGame.Views;
using System.Linq;
using TriviaGame.Services;
using TriviaGame.Models;
using System.Windows;

namespace TriviaGame.ViewModels
{
    //Clase para manejar la vista de las diferentes pantallas (menu, juego, score)
    //Esta clase es la que se va a conectar en el mainWindow.xaml.cs
    public class mainControlViewModel : propertiesChangesViewModel
    {
        private object currentView;
        private queryService queryService;
        private Random random;
        private readonly SocketClientService _socketClient;

        private int aciertosTotal = 0;
        private int erroresTotal = 0;
        private int puntuacionTotal = 0;
        private int idCategoriaActual = 0;
        private string categoriaActual = string.Empty;

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
            _socketClient = new SocketClientService();
            _socketClient.OnGameOver += OnGameOverReceived;
            CurrentView = new loginViewModel(
                this,
                startGame: async (categoryId) => await selectedCategory(categoryId),
                scoreMenu: () => CurrentView = new finalScoreViewModel(IrAMenuSync, 0)
            );
        }

        public async Task selectedCategory(string categoryId)
        {

            if (!int.TryParse(categoryId, out int idCategoria))
            {
                return;
            }

            idCategoriaActual = idCategoria;

            await AbrirSalaEspera(idCategoria);
        }

        private async Task AbrirSalaEspera(int idCategoria)
        {

            var salaEspera = new SalaEspera(
                idCategoria.ToString(),
                jugadorActual,
                async (questionsJson) =>
                {
                    await IniciarJuego(idCategoria, questionsJson);
                },
                _socketClient
            );

            CurrentView = salaEspera;
        }

        private async Task IniciarJuego(int idCategoria, string questionsJson = null)
        {
            aciertosTotal = 0;
            erroresTotal = 0;
            puntuacionTotal = 0;

            categoriaActual = await queryService.GetNombreCategoria(idCategoria);

            List<questionModel> preguntas;
            if (!string.IsNullOrWhiteSpace(questionsJson))
            {
                preguntas = ParseQuestionsFromServer(questionsJson, idCategoria);
            }
            else
            {
                preguntas = await LoadQuestionsFromApi(idCategoria);
            }

            if (preguntas == null || preguntas.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[IniciarJuego] ERROR - No hay preguntas para categoría {idCategoria}");
                return;
            }

            MostrarPregunta(preguntas, 0);
        }

        private async Task<List<questionModel>> LoadQuestionsFromApi(int idCategoria)
        {
            var preguntasRaw = await queryService.GetPreguntas(idCategoria);
            if (preguntasRaw == null || preguntasRaw.Count == 0)
            {
                return new List<questionModel>();
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

            return preguntas;
        }

        private List<questionModel> ParseQuestionsFromServer(string questionsJson, int idCategoria)
        {
            try
            {
                var preguntas = new List<questionModel>();
                var elementos = JsonSerializer.Deserialize<List<JsonElement>>(questionsJson);
                if (elementos == null) return preguntas;

                foreach (var questionElement in elementos)
                {
                    var respuestas = new List<answerModel>();
                    if (questionElement.TryGetProperty("answers", out JsonElement answersElement))
                    {
                        foreach (var answerElement in answersElement.EnumerateArray())
                        {
                            respuestas.Add(new answerModel
                            {
                                answer = answerElement.GetProperty("textRespuesta").GetString() ?? string.Empty,
                                isCorrect = answerElement.GetProperty("EsCorrecta").GetBoolean(),
                                mediaPath = answerElement.GetProperty("rutaRespuesta").GetString() ?? string.Empty,
                                answerType = answerElement.GetProperty("tipoRespuesta").GetString() ?? "TEXT"
                            });
                        }
                    }

                    string tipo = respuestas.FirstOrDefault()?.answerType?.ToUpper() ?? "TEXT";
                    questionModel pregunta = tipo switch
                    {
                        "SOUND" => new audioQuestionModel { pathAudio = respuestas.FirstOrDefault()?.mediaPath },
                        "IMG" => new imageQuestionModel { pathImage = respuestas.FirstOrDefault()?.mediaPath },
                        _ => new textQuestionModel()
                    };

                    pregunta.question = questionElement.GetProperty("nomPregunta").GetString() ?? string.Empty;
                    pregunta.categoryId = idCategoria;
                    pregunta.points = questionElement.TryGetProperty("puntuacionPregunta", out var pointsElement) ? pointsElement.GetInt32() : 0;
                    pregunta.answers = respuestas;
                    preguntas.Add(pregunta);
                }

                return preguntas;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ParseQuestionsFromServer] Error parseando preguntas: {ex.Message}");
                return new List<questionModel>();
            }
        }

        private void MostrarPregunta(List<questionModel> preguntas, int index)
        {
            if (index >= preguntas.Count)
            {
                _ = _socketClient.SendGameFinishedAsync(puntuacionTotal, preguntas[0].categoryId);
                System.Diagnostics.Debug.WriteLine($"[MostrarPregunta] Juego terminado, puntaje={puntuacionTotal}. Esperando game_over del servidor...");
                CurrentView = new EsperandoResultados();
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

        /// <summary>
        /// Método sincrónico que se llama desde UI (sin async support directo)
        /// Dispara la desconexión de forma asincrónica sin bloquear
        /// </summary>
        public void IrAMenuSync()
        {
            _ = IrAMenuAsync();
        }

        /// <summary>
        /// Método async que maneja la desconexión correctamente
        /// </summary>
        private async Task IrAMenuAsync()
        {
            System.Diagnostics.Debug.WriteLine("[IrAMenu] Iniciando desconexión...");

            await DisconnectSocket();

            System.Diagnostics.Debug.WriteLine("[IrAMenu] Desconexión completada, cambiando vista...");

            CurrentView = new mainMenuViewModel(
                startGame: async (categoryId) => await selectedCategory(categoryId),
                scoreMenu: () => CurrentView = new finalScoreViewModel(IrAMenuSync, idCategoriaActual)
            );
        }

        /// <summary>
        /// Método async que maneja la desconexión del socket
        /// </summary>
        private async Task DisconnectSocket()
        {
            try
            {
                if (_socketClient != null && _socketClient.IsConnected)
                {
                    System.Diagnostics.Debug.WriteLine("[DisconnectSocket] Desconectando del servidor...");
                    await _socketClient.DisconnectAsync();
                    System.Diagnostics.Debug.WriteLine("[DisconnectSocket] Desconexión completada exitosamente");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[DisconnectSocket] Socket ya estaba desconectado");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DisconnectSocket] Error desconectando: {ex.Message}");
            }
        }

        private void OnGameOverReceived(object sender, GameOverEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[OnGameOverReceived] Mostrar Score con {e.FinalScores.Count} jugadores");

            Application.Current.Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("[OnGameOverReceived] Mostrando Score en hilo UI");
                CurrentView = new Score(e.FinalScores, IrAMenuSync);
            });
        }
    }
}