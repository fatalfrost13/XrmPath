using System;
using System.Collections.Generic;
using System.Linq;

namespace XrmPath.Helpers.Utilities
{
    public static class ArrayUtility
    {

        /// <summary>
        /// Checks to see if a value exists in a csv
        /// </summary>
        /// <param name="value"></param>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static bool ValueExistsInCsv(this string value, string csv)
        {
            var exists = false;
            if (!string.IsNullOrEmpty(csv) && !string.IsNullOrEmpty(value))
            {
                if (csv.Contains(","))
                {
                    var array = csv.Split(',').ToArray();
                    exists = array.Contains(value);
                }
                else if (csv == value)
                {
                    exists = true;
                }
            }
            return exists;
        }

        /// <summary>
        /// Checks to see if a value in csv1 exists in csv2
        /// </summary>
        /// <param name="csv1"></param>
        /// <param name="csv2"></param>
        /// <returns></returns>
        public static bool CsvValueMatch(this string csv1, string csv2)
        {
            var exists = false;
            if (!string.IsNullOrEmpty(csv1) && !string.IsNullOrEmpty(csv2))
            {
                var array = csv1.Split(',').ToArray();
                var array2 = csv2.Split(',').ToArray();

                foreach (var item1 in from item1 in array from item2 in array2.Where(item2 => item1 == item2 && item2 != "") select item1)
                {
                    exists = true;
                }
            }
            return exists;
        }

        public static bool ContainsAll(this IEnumerable<int> sourceIntList, IEnumerable<int> checkIntList)
        {
            var containsAll = true;
            if (sourceIntList == null)
            {
                sourceIntList = Enumerable.Empty<int>();
            }
            if (checkIntList == null)
            {
                checkIntList = Enumerable.Empty<int>();
            }

            var sourceList = sourceIntList.ToList();
            foreach (var checkInt in checkIntList)
            {
                if (!sourceList.Contains(checkInt))
                {
                    containsAll = false;
                    break;
                }
            }

            return containsAll;
        }

        public static bool ContainsOne(this IEnumerable<int> sourceIntList, IEnumerable<int> checkIntList)
        {
            var containsOne = false;
            if (sourceIntList == null)
            {
                sourceIntList = Enumerable.Empty<int>();
            }
            if (checkIntList == null)
            {
                checkIntList = Enumerable.Empty<int>();
            }

            var sourceList = sourceIntList.ToList();
            foreach (var checkInt in checkIntList)
            {
                if (sourceList.Contains(checkInt))
                {
                    containsOne = true;
                    break;
                }
            }
            return containsOne;
        }

        public static bool ContainsIgnoreCase(this List<string> stringList, string value)
        {
            if (stringList.Any() && stringList.Select(i => i.ToLower()).Contains(value.ToLower()))
            {
                return true;
            }
            return false;
        }
        public static bool ContainsIgnoreCase(this ISet<string> stringList, string value)
        {
            if (stringList.Any() && stringList.Select(i => i.ToLower()).Contains(value.ToLower()))
            {
                return true;
            }
            return false;
        }
        public static bool ContainsIgnoreCase(this string stringValue, string value)
        {
            if (stringValue.ToLower().Contains(value.ToLower()))
            {
                return true;
            }
            //else if(characterMatch)
            //{
            //    //try each word in value
            //    var searchWordList = stringValue.Split(' ').ToList();
            //    var valueItems = value.Split(' ').ToList();
            //    if (searchWordList.Select(i => i.ToLower()).ContainsAny(valueItems.Select(i => i.ToLower())))
            //    {
            //        return true;
            //    }
            //}
            return false;
        }

        public static bool ContainsStringMatch(this string stringValue, string value, bool removeIgnored = true)
        {
            var searchWordList = stringValue.Split(' ').ToList();
            var valueItems = value.Split(' ').ToList();

            if (removeIgnored)
            {
                searchWordList = searchWordList.RemoveIgnoredCharacters();
                valueItems = valueItems.RemoveIgnoredCharacters();
            }

            if (searchWordList.Select(i => i.ToLower()).Intersect(valueItems.Select(i => i.ToLower())).Any())
            {
                return true;
            }
            return false;
        }

        private static readonly string[] ignoreCharacters = { "and", "the", "-", "of", "a" };
        public static List<string> RemoveIgnoredCharacters(this List<string> stringList)
        {
            var filteredList = stringList.Where(i => !ignoreCharacters.Contains(i.ToLower())).ToList();
            return filteredList;
        }

        /// <summary>
        /// Will split string that is not enclosed in quotes
        /// If no quotes exist, it will return
        /// </summary>
        /// <returns></returns>
        public static List<string> SmartSplit(this string word, char delimiter = ' ', bool splitNoQuotes = true)
        {
            //string word = "WordOne \"Word Two\"";
            
            if (word.Contains("\"") || splitNoQuotes)
            {
                var result = word.Split('"')
                    .Select((element, index) => index % 2 == 0  // If even index
                        ? element.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)  // Split the item
                        : new[] { element })  // Keep the entire item
                    .SelectMany(element => element).ToList();
                return result;
            }
            else
            {
                var result = new List<string> { word };
                return result;
            }
            
        }
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
    }
}