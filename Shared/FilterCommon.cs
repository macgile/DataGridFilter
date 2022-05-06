#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FIlterConsole
// Projet     : FilterDataGrid
// File       : FilterCommon.cs
// Created    : 02/05/2022
// 

#endregion

using System;
using System.Collections;

namespace FilterDataGrid
{
    public class FilterCommon
    {
        #region Public Properties

        public string FieldName { get; set; }

        public Type FieldType { get; set; }

        public bool IsFiltered { get; set; }

        public FilterCommon(int count)
        {
            PreviousItems = new BitArray(count);
            PreviousItems.SetAll(false);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public BitArray PreviousItems { get; set; }

       // public int ItemsCount => PreviousItems.Count;


        #endregion Public Properties
    }
}