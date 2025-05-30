using System;
using System.Windows.Input;

namespace EasySave_From_ProSoft.Utils
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged;
        // Remove comment when moving to WPF
        //{
        //    add    { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute    = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter)    => _execute(parameter);
    }
}
