using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TriviaGame.ViewModels
{
    class propertiesChangesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
