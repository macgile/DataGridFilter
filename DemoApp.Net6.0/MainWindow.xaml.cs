using System;
using System.Diagnostics;
using System.Windows;
using SharedModelView.ModelView;

namespace DemoApp.Net6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
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
