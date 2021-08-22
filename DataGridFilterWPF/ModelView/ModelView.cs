#region (c) 2019 Gilles Macabies All right reserved

//   Author     : Gilles Macabies
//   Solution   : DataGridFilter
//   Projet     : DataGridFilter
//   File       : ModelView.cs
//   Created    : 31/10/2019

#endregion (c) 2019 Gilles Macabies All right reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

// ReSharper disable MemberCanBePrivate.Global

namespace DemoApplication.ModelView
{
   public class ModelView : INotifyPropertyChanged
   {
      #region Public Constructors

      public ModelView()
      {
         FillData();
      }

      #endregion Public Constructors

      #region Command

      /// <summary>
      ///     Refresh all
      /// </summary>
      public ICommand RefreshCommand => new DelegateCommand(RefreshData);

      #endregion Command

      #region Private Fields

      private ICollectionView collView;

      private string search;

      #endregion Private Fields

      #region Public Properties

      public ObservableCollection<Employe> Employes { get; set; }
      public ObservableCollection<Employe> FilteredList { get; set; }

      /// <summary>
      /// Global filter
      /// </summary>
      public string Search
      {
         get => search;
         set
         {
            search = value;

            collView.Filter = e =>
            {
               var item = (Employe)e;
               return item != null && ((item.LastName?.StartsWith(search, StringComparison.OrdinalIgnoreCase) ?? false)
                                           || (item.FirstName?.StartsWith(search, StringComparison.OrdinalIgnoreCase) ?? false));
            };

            collView.Refresh();

            FilteredList = new ObservableCollection<Employe>(collView.OfType<Employe>().ToList());

            OnPropertyChanged("Search");
            OnPropertyChanged("FilteredList");
         }
      }

      #endregion Public Properties

      #region Public Events

      public event PropertyChangedEventHandler PropertyChanged;

      #endregion Public Events

      #region Private Methods

      /// <summary>
      /// Fill data
      /// </summary>
      private void FillData()
      {
         search = "";

         var employe = new List<Employe>();

         // number of elements to be generated
         const int @int = 100000;

         // for distinct lastname set "true" at CreateRandomEmployee(true)
         for (var i = 0; i < @int; i++)
            employe.Add(RandomGenerator.CreateRandomEmployee(true));

         Employes = new ObservableCollection<Employe>(employe.AsParallel().OrderBy(o => o.LastName));

         FilteredList = new ObservableCollection<Employe>(Employes);
         collView = CollectionViewSource.GetDefaultView(FilteredList);

         OnPropertyChanged("Search");
         OnPropertyChanged("Employes");
         OnPropertyChanged("FilteredList");
      }

      private void OnPropertyChanged(string propertyname)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
      }

      /// <summary>
      /// refresh data
      /// </summary>
      /// <param name="obj"></param>
      private void RefreshData(object obj)
      {
         FillData();
      }

      #endregion Private Methods
   }
}