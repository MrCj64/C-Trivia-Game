using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TriviaGame.ViewModels
{
    public class ImageGameViewModel : propertiesChangesViewModel
    {
        private readonly List<Dictionary<string, object>> _preguntas;
        private readonly Action _onFinished;
        private int _index = 0;

        public int Aciertos { get; private set; } = 0;
        public int Errores { get; private set; } = 0;

        private string _textoPregunta = "";
        public string TextoPregunta
        {
            get => _textoPregunta;
            set { _textoPregunta = value; onPropertyChanged(); }
        }

        private string _progreso = "";
        public string Progreso { get => _progreso; set { _progreso = value; onPropertyChanged(); } }

        private string _rutaA = "", _rutaB = "", _rutaC = "", _rutaD = "";
        public string RutaA { get => _rutaA; set { _rutaA = value; onPropertyChanged(); } }
        public string RutaB { get => _rutaB; set { _rutaB = value; onPropertyChanged(); } }
        public string RutaC { get => _rutaC; set { _rutaC = value; onPropertyChanged(); } }
        public string RutaD { get => _rutaD; set { _rutaD = value; onPropertyChanged(); } }

        private string _colorA = "Transparent", _colorB = "Transparent",
                       _colorC = "Transparent", _colorD = "Transparent";
        public string ColorA { get => _colorA; set { _colorA = value; onPropertyChanged(); } }
        public string ColorB { get => _colorB; set { _colorB = value; onPropertyChanged(); } }
        public string ColorC { get => _colorC; set { _colorC = value; onPropertyChanged(); } }
        public string ColorD { get => _colorD; set { _colorD = value; onPropertyChanged(); } }

        public ICommand ResponderCommand { get; }

        public ImageGameViewModel(List<Dictionary<string, object>> preguntas, Action onFinished)
        {
            _preguntas = preguntas;
            _onFinished = onFinished;
            ResponderCommand = new RelayCommand((obj) => Responder(obj as string));
            CargarPregunta();
        }

        private void CargarPregunta()
        {
            if (_index >= _preguntas.Count) { _onFinished?.Invoke(); return; }

            ResetColores();
            var pregunta = _preguntas[_index];
            var respuestas = (List<Dictionary<string, object>>)pregunta["respuestas"];

            TextoPregunta = pregunta["nomPregunta"].ToString();
            Progreso = $"Pregunta {_index + 1} de {_preguntas.Count}";

            RutaA = RutaAbsoluta(respuestas.Count > 0 ? respuestas[0]["rutaRespuesta"]?.ToString() ?? "" : "");
            RutaB = RutaAbsoluta(respuestas.Count > 1 ? respuestas[1]["rutaRespuesta"]?.ToString() ?? "" : "");
            RutaC = RutaAbsoluta(respuestas.Count > 2 ? respuestas[2]["rutaRespuesta"]?.ToString() ?? "" : "");
            RutaD = RutaAbsoluta(respuestas.Count > 3 ? respuestas[3]["rutaRespuesta"]?.ToString() ?? "" : "");
        }

        private void Responder(string opcion)
        {
            var respuestas = (List<Dictionary<string, object>>)_preguntas[_index]["respuestas"];
            int elegido = opcion switch { "A" => 0, "B" => 1, "C" => 2, "D" => 3, _ => -1 };
            if (elegido < 0 || elegido >= respuestas.Count) return;

            bool esCorrecta = (bool)respuestas[elegido]["EsCorrecta"];
            MostrarFeedback(respuestas, elegido);
            if (esCorrecta) Aciertos++; else Errores++;

            var timer = new System.Windows.Threading.DispatcherTimer
            { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => { timer.Stop(); _index++; CargarPregunta(); };
            timer.Start();
        }

        private void MostrarFeedback(List<Dictionary<string, object>> respuestas, int elegido)
        {
            string[] c = { "Transparent", "Transparent", "Transparent", "Transparent" };
            for (int i = 0; i < respuestas.Count && i < 4; i++)
            {
                if ((bool)respuestas[i]["EsCorrecta"]) c[i] = "#FF4CAF50";
                else if (i == elegido) c[i] = "#FFF44336";
            }
            ColorA = c[0]; ColorB = c[1]; ColorC = c[2]; ColorD = c[3];
        }

        private void ResetColores() =>
            ColorA = ColorB = ColorC = ColorD = "Transparent";
        private string RutaAbsoluta(string rutaRelativa)
        {
            if (string.IsNullOrEmpty(rutaRelativa)) return "";
            return System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                rutaRelativa.Replace("/", "\\"));
        }
    }
}
