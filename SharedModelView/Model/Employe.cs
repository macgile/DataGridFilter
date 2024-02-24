#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FilterDataGrid
// Projet     : DemoApp.Net7.0
// File       : Employe.cs
// Created    : 20/05/2023
// 

#endregion

using System;

// ReSharper disable CheckNamespace
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable TooManyDependencies
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable MemberCanBePrivate.Global

namespace SharedModelView
{
    public class Employe
    {
        #region Public Constructors

        public Employe(string lastName, string firstName, double? salary, int? age, DateTime? startDate,
            bool? manager = false, Departments department = Departments.None)
        {
            LastName = lastName;
            FirstName = firstName;
            Salary = salary;
            Age = age;
            StartDate = startDate;
            Manager = manager;
            Department = department;
        }

        #endregion Public Constructors

        #region Public Properties

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? Manager { get; set; }
        public double? Salary { get; set; }
        public int? Age { get; set; }
        public DateTime? StartDate { get; set; }
        public Departments Department { get; set; }

        #endregion Public Properties
    }

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

}