using System;
using System.Windows.Input;

namespace StellarisModManager
{
    public class Command : ICommand
    {
        private readonly Action<object> _action;
        private readonly Func<object, bool> _check;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }


        public Command(Action<object> action, Func<object, bool> check)
        {
            this._action = action;
            this._check = check;
        }

        public bool CanExecute(object parameter) => this._check?.Invoke(parameter) == true;

        public void Execute(object parameter) => this._action?.Invoke(parameter);

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
