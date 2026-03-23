using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGame.ViewModels
{
    public class finalScoreViewModel : propertiesChangesViewModel
    {
        public RelayCommand VolverMenuCommand { get; }

        public finalScoreViewModel(Action volverAMenu)
        {
            VolverMenuCommand = new RelayCommand(volverAMenu);
        }
    }
}
