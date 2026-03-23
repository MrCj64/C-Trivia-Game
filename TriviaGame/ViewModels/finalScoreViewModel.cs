using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGame.ViewModels
{
    public class finalScoreViewModel : propertiesChangesViewModel
    {
        public RelayCommand VolverMenuCommand { get; }
        public string Categoria { get; }
        public int Aciertos { get; }
        public int Errores { get; }
        public int Puntuacion { get; }

        public finalScoreViewModel(Action volverAMenu, string categoria, int aciertos, int errores)
        {
            VolverMenuCommand = new RelayCommand(volverAMenu);
            Categoria = categoria;
            Aciertos = aciertos;
            Errores = errores;
            Puntuacion = aciertos * 10;
        }
    }
}
