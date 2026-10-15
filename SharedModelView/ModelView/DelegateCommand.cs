#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FilterDataGrid
// Projet     : DemoApp.Net6.0
// File       : DelegateCommand.cs
// Created    : 13/11/2022
//

#endregion

using System;
using System.Windows.Input;

// ReSharper disable UnusedMember.Global

namespace SharedModelView.ModelView
{
    /// <summary>
    ///     DelegateCommand
    /// </summary>
    public class DelegateCommand(Action<object> execute,
        Predicate<object> canExecute = null) : ICommand
    {
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