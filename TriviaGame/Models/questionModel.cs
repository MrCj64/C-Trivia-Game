using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaGame.Models
{
    public abstract class questionModel
    {
        public int Id { get; set; }
        public int categoryId { get; set; }
        public int points { get; set; }
        public string question { get; set; }
        public string category { get; set; }
        public List<answerModel> answers { get; set; } = new List<answerModel>();
    }
}
