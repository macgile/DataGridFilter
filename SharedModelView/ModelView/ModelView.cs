#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FilterDataGrid
// Projet     : SharedModelView
// File       : ModelView.cs
// Created    : 13/11/2022
//

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SharedModelView.ModelView
{
    public class ModelView : INotifyPropertyChanged
    {
        #region Public Constructors

        public ModelView(int i = 10_000)
        {
            count = i;
            SelectedItem = count;
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Private Fields

        private ICollectionView collView;
        private int count;
        private string search = string.Empty;

        #endregion Private Fields

        #region Public Properties

        public ObservableCollection<Employe> Employes { get; set; }

        public ObservableCollection<Employe> FilteredList { get; set; }

        public int[] NumberItems { get; } =
        {
            10, 100, 1000, 10_000, 100_000, 500_000, 1_000_000
        };

        /// <summary>
        ///     Refresh all
        /// </summary>
        public ICommand RefreshCommand => new DelegateCommand(RefreshData);

        /// <summary>
        ///     Global filter
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
                    return item != null &&
                           (item.LastName != null && item.LastName.StartsWith(search, StringComparison.OrdinalIgnoreCase)
                            || item.FirstName != null && item.FirstName.StartsWith(search, StringComparison.OrdinalIgnoreCase));
                };

                if (collView != null)
                {
                    collView.Refresh();
                    FilteredList = new ObservableCollection<Employe>(collView.OfType<Employe>());
                }

                OnPropertyChanged(nameof(Search));
                OnPropertyChanged(nameof(FilteredList));
            }
        }

        public int SelectedItem
        {
            get => count;
            set
            {
                count = value;
                OnPropertyChanged(nameof(SelectedItem));
                Task.Run(FillData);
            }
        }

        #endregion Public Properties

        #region Private Methods

        /// <summary>
        ///     Fill data
        /// </summary>
        private async void FillData()
        {
            search = "";

            var employe = new List<Employe>(count);
            var countries = new Countries();

            // for distinct lastname set "true" at CreateRandomEmployee(true)
            await Task.Run(() =>
            {
                for (var i = 0; i < count; i++)
                    employe.Add(RandomGenerator.CreateRandomEmployee(true, countries));
            });

            Employes = new ObservableCollection<Employe>(employe);
            FilteredList = new ObservableCollection<Employe>(employe);
            collView = CollectionViewSource.GetDefaultView(FilteredList);

            OnPropertyChanged("Search");
            OnPropertyChanged("Employes");
            OnPropertyChanged("FilteredList");
        }

        public void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        /// <summary>
        ///     refresh data
        /// </summary>
        /// <param name="obj"></param>
        private void RefreshData(object obj)
        {
            collView = CollectionViewSource.GetDefaultView(new object());
            Task.Run(FillData);
        }

        #endregion Private Methods
    }
}