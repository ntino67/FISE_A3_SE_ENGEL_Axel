using System;
using System.Windows.Input;

namespace Core.Model.Interfaces
{
    public interface ICommandFactory
    {
        ICommand Create(Action<object> execute, Predicate<object> canExecute = null);
        ICommand Create<T>(Action<T> execute, Predicate<T> canExecute = null);
    }
}