#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FilterDataGrid
// Projet     : DemoApp.Net8.0, DemoApp.Net9.0, DemoApp.Net10.0
// File       : Employe.cs
// Created    : 20/05/2023
//

#endregion

using System;
using System.Collections.ObjectModel;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SharedModelView
{
    public enum Departments
    {
        None,
        Administration,
        Finance,
        HumanResources,
        Logistics,
        Marketing,
        Production,
        Sales
    }

    public class Countries : ObservableCollection<Country>
    {
        #region Public Constructors

        public Countries()
        {
            Add(new Country { Id = 0, Name = "Chinese", Region = "Asia & Pacific" });
            Add(new Country { Id = 1, Name = "Dutch", Region = "Europe" });
            Add(new Country { Id = 2, Name = "English", Region = "Europe" });
            Add(new Country { Id = 3, Name = "French", Region = "Europe" });
            Add(new Country { Id = 4, Name = "German", Region = "Europe" });
            Add(new Country { Id = 5, Name = "Hebrew", Region = "Middle east" });
            Add(new Country { Id = 6, Name = "Hungarian", Region = "Europe" });
            Add(new Country { Id = 7, Name = "Italian", Region = "Europe" });
            Add(new Country { Id = 8, Name = "Japanese", Region = "Asia & Pacific" });
            Add(new Country { Id = 9, Name = "Polish", Region = "Europe" });
            Add(new Country { Id = 10, Name = "Portuguese", Region = "Europe" });
            Add(new Country { Id = 11, Name = "Russian", Region = "Europe" });
            Add(new Country { Id = 12, Name = "Spanish", Region = "Europe" });
            Add(new Country { Id = 13, Name = "Turkish", Region = "Europe" });
            Add(new Country { Id = 14, Name = "Ukrainian", Region = "Europe" });
        }

        #endregion Public Constructors

        #region Public Methods

        // not used
        public Country GetAt(int index)
        {
            return this[index];
        }

        #endregion Public Methods
    }

    public class Country
    {
        #region Public Properties

        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public int Id { get; set; }

        public string Name { get; set; }
        public string Region { get; set; }

        #endregion Public Properties
    }

    public class Employe(string lastName, string firstName, double? salary, int? age, DateTime? startDate,
        bool? manager = false, Departments department = Departments.None, int idCountry = 0, Country country = null)
    {
        #region Public Properties

        public int? Age { get; set; } = age;
        public Departments Department { get; set; } = department;
        public Country Country { get; set; } = country;
        public string FirstName { get; set; } = firstName;
        public int IdCountry { get; set; } = idCountry;
        public string LastName { get; set; } = lastName;
        public bool? Manager { get; set; } = manager;
        public double? Salary { get; set; } = salary;
        public DateTime? StartDate { get; set; } = startDate;

        #endregion Public Properties
    }
}