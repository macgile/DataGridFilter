using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DemoApplication;

namespace DataGridFilterWPF
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
            Debug.WriteLine("FirstChanceException event raised in {0}: {1}",
                AppDomain.CurrentDomain.FriendlyName, e.Exception.Message);
         };
#endif
         DataContext = new DemoApplication.ModelView.ModelView();
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