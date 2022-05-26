#region (c) 2019 Gilles Macabies All right reserved

//   Author     : Gilles Macabies
//   Solution   : DataGridFilter
//   Projet     : DataGridFilter
//   File       : MainWindow.xaml.cs
//   Created    : 31/10/2019

#endregion (c) 2019 Gilles Macabies All right reserved

using System;
using System.Diagnostics;

namespace DemoApplication
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            AppDomain.CurrentDomain.FirstChanceException += (source, e) =>
            {
                
                Debug.WriteLine($"FirstChanceException event raised in " +
                                $"{AppDomain.CurrentDomain.FriendlyName}: {e.Exception.Message} {source}");
            };
#endif

            DataContext = new ModelView.ModelView();
        }

        #endregion Public Constructors
    }
}
