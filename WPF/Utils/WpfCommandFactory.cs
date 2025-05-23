using System;
using System.Windows.Input;
using Core.Model.Interfaces;

namespace WPF.Utils
{
    public class WpfCommandFactory : ICommandFactory
    {
        public ICommand Create(Action<object> execute, Predicate<object> canExecute = null)
            => new RelayCommand(execute, canExecute);

        public ICommand Create<T>(Action<T> execute, Predicate<T> canExecute = null)
            => new RelayCommand<T>(execute, canExecute);
    }

}