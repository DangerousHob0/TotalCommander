using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TotalCommander
{
    class RelayCommand<T> : ICommand
    {
        readonly Action<T> action;
        readonly Predicate<object> canExecute;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> action, Predicate<object> canExecute = null)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            action((T)parameter);
        }
    }
}
