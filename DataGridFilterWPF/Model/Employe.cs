#region (c) 2019 Gilles Macabies All right reserved

//   Author     : Gilles Macabies
//   Solution   : DataGridFilter
//   Projet     : DataGridFilter
//   File       : Employe.cs
//   Created    : 31/10/2019

#endregion (c) 2019 Gilles Macabies All right reserved

using System;
// ReSharper disable CheckNamespace

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable TooManyDependencies
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable MemberCanBePrivate.Global

namespace DemoApplication
{
    public class Employe
    {
        #region Public Constructors

        public Employe()
        {
        }

        public Employe(string lastName, string firstName, int? salary, DateTime? startDate, bool? manager = false)
        {
            LastName = lastName;
            FirstName = firstName;
            Salary = salary;
            StartDate = startDate;
            Manager = manager;
        }

        #endregion Public Constructors

        #region Public Properties

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? Manager { get; set; }
        public int? Salary { get; set; }
        public DateTime? StartDate { get; set; }

        #endregion Public Properties
    }
}
