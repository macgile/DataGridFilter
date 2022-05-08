// Author     : Gilles Macabies
// Solution   : FIlterConsole
// Projet     : FilterDataGrid
// File       : FilterManager.cs
// Created    : 03/05/2022

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FilterDataGrid
{
    public class FilterManager
    {
        #region Private Fields

        private readonly int itemCount;
        private int index;

        #endregion Private Fields

        #region Public Constructors

        public FilterManager(int count)
        {
            itemCount = count;
            StackItems = new BitArray(count);
            StackItems.SetAll(true);
            Stock = new BitArray(count);
            Stock.SetAll(false);
        }

        #endregion Public Constructors

        #region Public Properties

        // ReSharper disable once MemberCanBePrivate.Global
        public FilterCommon CurrentFilter { get; set; }

        public bool HasPrecedent { get; set; }
        public bool IsLast => Queue.Count == 1 && HasPrecedent;
        public FilterCommon LastFilter => Queue.LastOrDefault();
        public List<FilterCommon> Queue { get; } = new List<FilterCommon>();
        public BitArray StackItems { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public BitArray Stock { get; }

        #endregion Public Properties

        #region Private Properties

        private int QueueCount => Queue.Count;

        #endregion Private Properties

        #region Public Methods

        // get item state
        // loop filter
        public bool Current()
        {
            if (index >= StackItems.Count)
                index = 0;
            return StackItems[index++];
        }

        public void Dequeue(FilterCommon c)
        {
            if (c == null) throw new ArgumentException(@"The FilterCommon cannot be null", nameof(c));
            Queue.Remove(c);

            // warning: is it necessary?
            CurrentFilter = null;
        }

        public void Enqueue()
        {
            HasPrecedent = Queue.Count > 0;
            if (CurrentFilter == null) return;

            Queue.Remove(CurrentFilter);
            Queue.Add(CurrentFilter);
            CurrentFilter.IsFiltered = true;
        }

        public void PrintState(string message, bool hidden = false)
        {
            if (!hidden || StackItems.Count > 10) return;

            Debug.WriteLine(message);

            var titles = new List<string>
            {
                "  | Pile  "
            };

            titles.AddRange(Queue.Select(q => q.FieldName)
                .AsEnumerable()
                .Select(q => $"{q,-6}"));

            var separator = "\r\n――+" + string.Concat(Enumerable.Repeat("――――――――+", QueueCount + 1));

            var entete = separator;
            entete += "\r\n" + string.Join(" | ", titles) + " |";

            var line = "";
            entete += separator;

            for (var i = 0; i < StackItems.Count; i++)
            {
                var items = new string[QueueCount + 1];

                //items[0] = $"{i} | {StackItems.Get(i),-6} | {Stock.Get(i),-6} ";
                items[0] = $"{i} | {StackItems.Get(i),-6} ";

                for (var j = 0; j < QueueCount; j++)
                {
                    items[j + 1] = $" {Queue[j].PreviousItems.Get(i),-6} ";
                    if (j == QueueCount - 1)
                        items[j + 1] += "|";
                }

                line += $"{string.Join("|", items)}";
                if (QueueCount == 0)
                    line += "|";

                line += separator + "\r\n";
            }

            Debug.WriteLine($"{entete}\r\n{line}");
        }

        public void SetCurrent(string fieldName, Type type)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (type == null) throw new ArgumentNullException(nameof(type));

            FilterCommon current;

            if ((current = Queue.FirstOrDefault(c => c.FieldName == fieldName)) == null)
            {
                current = new FilterCommon(itemCount)
                {
                    FieldName = fieldName,
                    FieldType = type
                };

                CurrentFilter = current;
            }
            else
            {
                CurrentFilter = current;
            }
        }

        #endregion Public Methods
    }
}