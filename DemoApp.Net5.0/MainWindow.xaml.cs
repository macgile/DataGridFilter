using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

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
            Debug.WriteLine("FirstChanceException event raised in {0}: {1}",
                AppDomain.CurrentDomain.FriendlyName, e.Exception.Message);
         };
#endif
         DataContext = new ModelView.ModelView();
      }

#endregion Public Constructors

      #region Private Methods

      /// <summary>
      /// Add line numbers
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
      {
         var index = e.Row.GetIndex() + 1;
         e.Row.Header = $"{index}";
      }

      #endregion Private Methods
   }
}