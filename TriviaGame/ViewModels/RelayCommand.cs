using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace TriviaGame.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Action _executeNonParameter;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute)
        {
            //_execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _executeNonParameter = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public RelayCommand(Action<object> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object parameter) => true; //_canExecute == null || _canExecute();

        public void Execute(object parameter)
        {
            if(_execute!= null)
                _execute(parameter);
            else 
                _executeNonParameter();
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
