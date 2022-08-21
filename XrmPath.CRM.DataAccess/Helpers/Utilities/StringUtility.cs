using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XrmPath.CRM.DataAccess.Utilities
{
    public static class StringUtility
    {
        public static ISet<string> StringToSet(this string values, char separator = ',')
        {
            var hashSet = !String.IsNullOrEmpty(values) ? new HashSet<string>(values.Split(separator), StringComparer.Ordinal) : new HashSet<string>();
            return hashSet;
        }

        public static ISet<int> StringToIntSet(this string values, char separator = ',')
        {
            var hashSet = !String.IsNullOrEmpty(values) ? new HashSet<int>(values.Split(separator).Select(Int32.Parse)) : new HashSet<int>();
            return hashSet;
        }

        public static IEnumerable<int> StringToEnumIntList(this string values, char separator = ',')
        {
            var intList = !String.IsNullOrEmpty(values) ? values.Split(separator).Select(Int32.Parse) : Enumerable.Empty<int>();
            return intList;
        }
        public static string FormatCurrency(double value, bool showDecimal = true)
        {
            var format = "{0:C}";
            if (!showDecimal)
            {
                format = "{0:C0}";
            }
            var formatCurrency = string.Format(CultureInfo.CurrentCulture, format, value);
            return formatCurrency;
        }

        public static bool ToBoolean(string value)
        {
            var formattedValue = value?.Trim().ToLower() ?? string.Empty;
            return (formattedValue == "1" || formattedValue == "yes" || formattedValue == "true");
        }

        public static Guid ToGuid(string value)
        {
            var guid = Guid.Empty;
            if (!string.IsNullOrEmpty(value))
            {
                Guid.TryParse(value, out guid);
            }
            return guid;
        }
    }
}