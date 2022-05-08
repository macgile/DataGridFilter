using System;
using System.Diagnostics;
using System.Windows;

namespace DemoAppNet5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
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
                Debug.WriteLine("FirstChanceException event raised in {0}: {1}\r\n{2}",
                 AppDomain.CurrentDomain.FriendlyName, e.Exception.Message, source);
            };
#endif
            var viewModel = new ModelView.ModelView(100_000);

            DataContext = viewModel;
        }

        #endregion Public Constructors
    }
}