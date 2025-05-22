using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FilterDataGrid.Extension
{
    public static class ItemsExtension
    {
        public static List<object> Distincts(this ItemCollection items, Type fieldType, string fieldName)
        {
            List<object> result = new List<object>();
            if (fieldType.IsDateOrTime())
            {
                if (fieldType == typeof(DateTime))
                    foreach (var item in items.Distincts_DateTime(fieldName))
                        result.Add(item);
                else if (fieldType == typeof(TimeSpan))
                    foreach (var item in items.Distincts_TimeSpan(fieldName))
                        result.Add(item);

#if NET6_0_OR_GREATER
                if (fieldType == typeof(DateOnly))
                    foreach (var item in items.Distincts_DateOnly(fieldName))
                        result.Add(item);
                else if (fieldType == typeof(TimeOnly))
                    foreach (var item in items.Distincts_TimeOnly(fieldName))
                        result.Add(item);
#endif
                return result;
            }

            return items.Cast<object>()
                                .Select(x => x.GetPropertyValue(fieldName))
                                .Distinct()
                                .ToList();
        }
        public static IEnumerable<DateTime?> Distincts_DateTime(this ItemCollection items, string fieldName)
        {
            return items.Cast<object>()
                        .Select(x => x.ToDateTime(fieldName)?.Date)
                        .Distinct();
        }
        public static IEnumerable<TimeSpan?> Distincts_TimeSpan(this ItemCollection items, string fieldName)
        {
            return items.Cast<object>()
                        .Select(x => x.ToTimeSpan(fieldName))
                        .Distinct();
        }

#if NET6_0_OR_GREATER
        public static IEnumerable<DateOnly?> Distincts_DateOnly(this ItemCollection items, string fieldName)
        {
            return items.Cast<object>()
                        .Select(x => x.ToDateOnly(fieldName))
                        .Distinct();
        }
        public static IEnumerable<TimeOnly?> Distincts_TimeOnly(this ItemCollection items, string fieldName)
        {
            return items.Cast<object>()
                        .Select(x => x.ToTimeOnly(fieldName))
                        .Distinct();
        }
#endif
    }
}
