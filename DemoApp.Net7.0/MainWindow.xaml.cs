#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FilterDataGrid
// Projet     : DemoApp.Net7.0
// File       : MainWindow.xaml.cs
// Created    : 04/12/2022
//

#endregion

using FilterDataGrid;
using SharedModelView.ModelView;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace DemoApp.Net7._0
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            AppDomain.CurrentDomain.FirstChanceException += (source, e) =>
            {
                Debug.WriteLine("FirstChanceException event raised in " +
                                $"{AppDomain.CurrentDomain.FriendlyName}: {e.Exception.Message} {source}");
            };
#endif

            DataContext = new ModelView();
           
        }

        #endregion Public Constructors
    }
}