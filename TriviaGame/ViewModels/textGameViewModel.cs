using System;
using System.Collections.Generic;
using System.Windows.Input;
using TriviaGame.Models;

namespace TriviaGame.ViewModels
{
    public class textGameViewModel : propertiesChangesViewModel
    {
        private readonly List<questionModel> _preguntas;
        private readonly Action _onFinished;
        private int _index = 0;
        public int Puntuacion { get; private set; } = 0;

        public int Aciertos { get; private set; } = 0;
        public int Errores { get; private set; } = 0;

        private string _textoPregunta = "";
        public string TextoPregunta
        {
            get => _textoPregunta;
            set { _textoPregunta = value; onPropertyChanged(); }
        }

        private string _progreso = "";
        public string Progreso
        {
            get => _progreso;
            set { _progreso = value; onPropertyChanged(); }
        }

        private string _textoA = "", _textoB = "", _textoC = "", _textoD = "";
        public string TextoA { get => _textoA; set { _textoA = value; onPropertyChanged(); } }
        public string TextoB { get => _textoB; set { _textoB = value; onPropertyChanged(); } }
        public string TextoC { get => _textoC; set { _textoC = value; onPropertyChanged(); } }
        public string TextoD { get => _textoD; set { _textoD = value; onPropertyChanged(); } }

        private string _colorA = "Transparent", _colorB = "Transparent",
                       _colorC = "Transparent", _colorD = "Transparent";
        public string ColorA { get => _colorA; set { _colorA = value; onPropertyChanged(); } }
        public string ColorB { get => _colorB; set { _colorB = value; onPropertyChanged(); } }
        public string ColorC { get => _colorC; set { _colorC = value; onPropertyChanged(); } }
        public string ColorD { get => _colorD; set { _colorD = value; onPropertyChanged(); } }

        public ICommand ResponderCommand { get; }

        public textGameViewModel(List<questionModel> preguntas, Action onFinished)
        {
            _preguntas = preguntas;
            _onFinished = onFinished;
            ResponderCommand = new RelayCommand(obj => Responder(obj as string));
            CargarPregunta();
        }

        private void CargarPregunta()
        {
            if (_index >= _preguntas.Count) { _onFinished?.Invoke(); return; }

            ResetColores();
            var pregunta = _preguntas[_index];
            var respuestas = pregunta.answers;

            TextoPregunta = pregunta.question;
            Progreso = $"Pregunta {_index + 1} de {_preguntas.Count}";

            TextoA = respuestas.Count > 0 ? respuestas[0].answer ?? "" : "";
            TextoB = respuestas.Count > 1 ? respuestas[1].answer ?? "" : "";
            TextoC = respuestas.Count > 2 ? respuestas[2].answer ?? "" : "";
            TextoD = respuestas.Count > 3 ? respuestas[3].answer ?? "" : "";
        }

        private void Responder(string opcion)
        {
            var respuestas = _preguntas[_index].answers;
            int elegido = opcion switch { "A" => 0, "B" => 1, "C" => 2, "D" => 3, _ => -1 };
            if (elegido < 0 || elegido >= respuestas.Count) return;

            if (respuestas[elegido].isCorrect)
            { 
                Aciertos++;
                Puntuacion += _preguntas[_index].points;
            }
            else 
                Errores++;
            MostrarFeedback(respuestas, elegido);

            var timer = new System.Windows.Threading.DispatcherTimer
            { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => { timer.Stop(); _index++; CargarPregunta(); };
            timer.Start();
        }

        private void MostrarFeedback(List<answerModel> respuestas, int elegido)
        {
            string[] c = { "Transparent", "Transparent", "Transparent", "Transparent" };
            for (int i = 0; i < respuestas.Count && i < 4; i++)
            {
                if (respuestas[i].isCorrect) c[i] = "#FF4CAF50";
                else if (i == elegido) c[i] = "#FFF44336";
            }
            ColorA = c[0]; ColorB = c[1]; ColorC = c[2]; ColorD = c[3];
        }

        private void ResetColores() =>
            ColorA = ColorB = ColorC = ColorD = "Transparent";
    }
}