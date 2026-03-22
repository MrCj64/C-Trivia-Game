using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaGame.ViewModels
{
    internal class audioGameViewModel : propertiesChangesViewModel
    {
        private List<Dictionary<string, object>> _preguntas;
        private int _currentQuestionIndex = 0;

        public List<Dictionary<string, object>> Preguntas
        {
            get => _preguntas;
            set
            {
                _preguntas = value;
                onPropertyChanged(nameof(Preguntas));
            }
        }

        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set
            {
                _currentQuestionIndex = value;
                onPropertyChanged(nameof(CurrentQuestionIndex));
            }
        }

        public audioGameViewModel(List<Dictionary<string, object>> preguntas)
        {
            Preguntas = preguntas;
            CurrentQuestionIndex = 0;
        }
    }
}
