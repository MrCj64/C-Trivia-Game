using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGame.Models
{
    internal abstract class questionModel
    {
        public List<string> questions = new List<string>();
        public List<string> answers = new List<string>();
        public string category { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
        public bool correctAnswer { get; set; }
        public int categoryId { get; set; }
        public int questionId { get; set; }
        public int answerId { get; set; }

        
    }
}
