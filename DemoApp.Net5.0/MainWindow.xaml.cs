using System;
using System.Diagnostics;
using System.Windows;
using SharedModelView.ModelView;

namespace DemoAppNet5
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
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
                
             Debug.WriteLine("FirstChanceException event raised in " +
                             $"{AppDomain.CurrentDomain.FriendlyName}: {e.Exception.Message} {source}");
         };
#endif
         DataContext = new ModelView();
      }

#endregion Public Constructors
   }
}