using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaGame.Models
{
    public class scoreModel
    {
        public string Jugador { get; set; }
        public string Categoria { get; set; }
        public int Puntuacion { get; set; }
        public int Aciertos { get; set; }
    }
}
