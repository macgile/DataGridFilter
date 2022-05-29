#region (c) 2019 Gilles Macabies All right reserved

//   Author     : Gilles Macabies
//   Solution   : DataGridFilter
//   Projet     : DataGridFilter
//   File       : DelegateCommand.cs
//   Created    : 05/11/2019

#endregion

using System;
using System.Windows.Input;
// ReSharper disable UnusedMember.Global

namespace DemoAppNet5.ModelView
{
    /// <summary>
    ///     DelegateCommand borrowed from
    ///     http://www.wpftutorial.net/DelegateCommand.html
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> canExecute;
        private readonly Action<object> execute;

        public DelegateCommand(Action<object> execute,
            Predicate<object> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region ICommand Members

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        #endregion
    }
}
