//   Author     : Gilles Macabies
//   Solution   : DataGridFilter
//   Projet     : DataGridFilter
//   File       : ModelView.cs
//   Created    : 31/10/2019

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

// ReSharper disable MemberCanBePrivate.Global

namespace DemoAppNet5.ModelView
{
    public class ModelView : INotifyPropertyChanged
    {
        #region Private Fields

        private ICollectionView collView;
        private int count;
        private string search;

        #endregion Private Fields

        #region Public Constructors

        public ModelView(int num)
        {
            count = num;
            Task.Run(FillData);
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public ObservableCollection<Employe> Employes { get; set; }

        public ObservableCollection<Employe> FilteredList { get; set; }

        public int NumberItems
        {
            get => count;
            set
            {
                count = value;
                Task.Run(FillData);
            }
        }

        /// <summary>
        ///     Refresh all
        /// </summary>
        public ICommand RefreshCommand => new DelegateCommand(RefreshData);

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

        #region Private Methods

        /// <summary>
        /// Fill data
        /// </summary>
        private async void FillData()
        {
            search = "";

            var employe = new List<Employe>();

            // for distinct lastname set "true" at CreateRandomEmployee(true)
            await Task.Run(() =>
            {
                for (var i = 0; i < count; i++)
                    employe.Add(RandomGenerator.CreateRandomEmployee(true));
            });

            Employes = new ObservableCollection<Employe>(employe/*.AsParallel().OrderBy(o => o.LastName)*/);
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
           // Employes?.Clear();
            Task.Run(FillData);
        }

        #endregion Private Methods
    }
}