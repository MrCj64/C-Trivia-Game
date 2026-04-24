using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaGame.Models
{
    internal class answerModel
    {
        //Cambiar a listas
        public int id { get; set; }
        public int idQuestion { get; set; }
        public string answer { get; set; }
        public bool isCorrect { get; set; }

    }
}
