#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FilterDataGrid
// Projet     : DemoApp.NET.Framework
// File       : MainWindow.xaml.cs
// Created    : 18/11/2022
//

#endregion

using SharedModelView.ModelView;
using System;
using System.Diagnostics;
using System.Windows;

namespace DemoApp.NET.Framework
{
    /// <summary>
    ///     Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
    }
}