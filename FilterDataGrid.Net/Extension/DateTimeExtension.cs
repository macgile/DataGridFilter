using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterDataGrid.Extension
{
    public static class DateTimeExtension
    {
        #region Conversion

        /// <summary>
        /// Convert an object to the specified type.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="type">The target type.</param>
        /// <returns>The converted object.</returns>
        public static object ConvertToType(this object value, Type type)
        {
            try
            {
                if (type.IsDateOrTime())
                {
                    return value.ConvertToType(type);
                }
                if (type.IsEnum)
                {
                    return Enum.Parse(type, value.ToString());
                }
                return Convert.ChangeType(value, type);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ConvertToType error: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Convert an object to the specified type.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="type">The target type.</param>
        /// <returns>The converted object.</returns>
        private static object ConvertToDateTime(object value, Type type)
        {
            if (type == typeof(DateTime))
                return DateTime.TryParse(value?.ToString(), out var dateTime) ? (object)dateTime : (object)(DateTime?)null;
#if NET6_0_OR_GREATER
            if (type == typeof(DateOnly))
                return DateOnly.TryParse(value?.ToString(), out var dateOnly) ? (object)dateOnly : (object)(DateOnly?)null;
            if (type == typeof(TimeOnly))
                return TimeOnly.TryParse(value?.ToString(), out var timeOnly) ? (object)timeOnly : (object)(TimeOnly?)null;
#endif
            return TimeSpan.TryParse(value?.ToString(), out var timeSpan) ? (object)timeSpan : (object)(TimeSpan?)null;
        }



        public static System.Nullable<TValue> CastField<TValue>(this object obj, string fieldName)
            where TValue : struct
        {
            return ((System.Nullable<TValue>)obj.GetPropertyValue(fieldName));
        }
        public static DateTime? ToDateTime(this object obj, string fieldName)
        {
            return CastField<DateTime>(obj, fieldName);
        }
        public static TimeSpan? ToTimeSpan(this object obj, string fieldName)
        {
            return obj.CastField<TimeSpan>(fieldName);
        }

#if NET6_0_OR_GREATER
        public static DateOnly? ToDateOnly(this object obj, string fieldName)
        {
            return obj.CastField<DateOnly>(fieldName);
        }
        public static TimeOnly? ToTimeOnly(this object obj, string fieldName)
        {
            return obj.CastField<TimeOnly>(fieldName);
        }

#endif
        #endregion

        #region Check

        public static bool IsDateOrTime(this Type type)
        {
            return IsDate(type) || IsTime(type);
        }
        public static bool IsDate(this Type type)
        {
#if NET6_0_OR_GREATER
            if (type == typeof(DateOnly) || type == typeof(DateOnly?))
                return true;
#endif
            return type == typeof(DateTime) || type == typeof(DateTime?);
        }
        public static bool IsTime(this Type type)
        {
#if NET6_0_OR_GREATER
            if (type == typeof(TimeOnly) || type == typeof(TimeOnly?))
                return true;
#endif
            return type == typeof(TimeSpan) || type == typeof(TimeSpan?);
        }
        #endregion
    }
}
