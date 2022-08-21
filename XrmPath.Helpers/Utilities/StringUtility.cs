using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XrmPath.Helpers.Utilities
{
    public static class StringUtility
    {
        public static string RemoveHtml(this string s)
        {
            var regex = new Regex(@"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", RegexOptions.Singleline);
            var test = regex.Replace(s, string.Empty).Replace("\n", " ").Replace("\r", " ").Trim();
            return test;
        }
        public static string GetNumberOfWords(this string s, int wordCount)
        {
            var cleanString = s.RemoveHtml();
            var countRegex = @"((?:(\S+\s+){1," + wordCount + @"})\w+)";
            var test = Regex.Match(cleanString, countRegex).Value;
            return test;
        }

        public static bool IsValidInteger(this string s)
        {
            var regex = new Regex(@"^\d+$", RegexOptions.Compiled);
            return regex.IsMatch(s);
        }
        public static readonly string PostalCodeRegex = @"^(([A-Za-z])(\d)([A-Za-z])([-\s]*)(\d)([A-Za-z])(\d))$";
        public static bool IsValidPostalCode(this string s)
        {
            var postalReg = new Regex(PostalCodeRegex);
            return postalReg.Match(s).Success;
        }
        public static string ToPostalCode(this string s)
        {
            var postalReg = new Regex(PostalCodeRegex);
            if (!string.IsNullOrWhiteSpace(s) && postalReg.Match(s).Success)
            {
                return postalReg.Replace(s, "$2$3$4 $6$7$8").ToUpper();
            }
            return s;
        }
        public static readonly string PhoneRegex = @"^(\+?1)?[-. ]?\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})[ -\.]?([e|E]?x?t?[ -\.]?([0-9]{1,5}))?$";
        public static string ToPhoneNumber(this string s)
        {
            var regexObj = new Regex(PhoneRegex);
            var phone = s.Replace(" ", "").Replace(".", "").Replace("-", "").Replace("(", "").Replace(")", "");
            if (!string.IsNullOrWhiteSpace(phone) && regexObj.IsMatch(phone) && phone.Length == 10)
            {
                return !string.IsNullOrWhiteSpace(regexObj.Match(phone).Groups[6].Value) ? regexObj.Replace(phone, "$2.$3.$4.x$6").Trim() : regexObj.Replace(phone, "$2.$3.$4").Trim();
            }
            return s;
        }
        public static readonly string WebRegex = @"^(http[s]?://)?((([a-zA-Z0-9-]+)\.)?([a-zA-Z0-9-.]+)*\.(([0-9]{1,3})|([a-zA-Z]{2,3})|(aero|asia|coop|info|jobs|mobi|museum|name|travel)))$";
        public static string ToWebUrl(this string s)
        {
            var regexObj = new Regex(WebRegex);
            if (!string.IsNullOrWhiteSpace(s) && regexObj.IsMatch(s)) return regexObj.Replace(s, "$2");
            return s;
        }
        public static readonly string PositiveCurrencyRegex = @"^\$?[+]?[1-9]+[0-9]{0,2}(?:,?[0-9]{3})*(?:\.[0-9]{2})?$";
        public static readonly string CurrencyRegex = @"^\$?[+-]?[0-9]{1,3}(?:,?[0-9]{3})*(?:\.[0-9]{2})?$";
        public static bool IsValidCurrency(this string s, out decimal value, string cultureInfo = "en-CA")
        {
            return decimal.TryParse(s,
                NumberStyles.Currency,
                CultureInfo.GetCultureInfo(cultureInfo),
                out value);
        }
        public static readonly string EmailRegex = "(^[_a-zA-Z0-9-]+(\\.[_a-zA-Z0-9-]+)*@[a-zA-Z0-9-]+(\\.[a-zA-Z0-9-]+)*\\.(([0-9]{1,3})|([a-zA-Z]{2,3})|(aero|coop|info|museum|name))$)";
        public static bool IsValidEmail(this string email)
        {
            var emailReg = new Regex(EmailRegex);
            return emailReg.Match(email).Success;
        }

        private const string CleanStringRegex = @"[^a-zA-Z0-9_]+";

        public static string CleanString(this string s)
        {
            return Regex.Replace(s, CleanStringRegex, string.Empty);
        }
        public static bool ContainsSpecialCharacters(this string s)
        {
            var stringReg = new Regex(CleanStringRegex, RegexOptions.Compiled);
            return stringReg.Match(s).Success;
        }
        public static string RemoveSpecialCharacters(this string str)
        {
            return Regex.Replace(str, CleanStringRegex, string.Empty, RegexOptions.Compiled);
        }
        public static int AsInt32(this string value)
        {
            int parsed;
            int.TryParse(value.Replace(",", ""), out parsed);
            return parsed;
        }

        public static string RemoveHtmlEntities(this string source)
        {
            return Regex.Replace(source, "&[a-zA-Z]+?;", string.Empty, RegexOptions.Compiled);
        }

        public static string SubstringWord(this string value, int length)
        {
            StringBuilder result = new StringBuilder(length);

            if (value.Length > length)
            {
                string[] words = value.Split(' ', '\n');
                for (int ix = 0; ix < words.Length; ix++)
                {
                    if ((result.Length + words[ix].Length) >= length)
                    {
                        break;
                    }
                    result.AppendFormat("{0} ", words[ix]);
                }
                result.Append("...");
            }
            else
            {
                result.Append(value);
            }
            return result.ToString();
        }

        public static string FormatRichText(this string text)
        {
            var richText = text;
            //richText = TemplateUtilities.ParseInternalLinks(richText);
            return richText;
        }

        public static string GetFullUrl(string fullUrl, string appendPath = "")
        {
            var uri = new Uri(fullUrl);
            //var host = uri.Host;
            var host = uri.Authority;
            var domain = $"http://{host}{appendPath}";
            if (fullUrl.Contains("https://"))
            {
                domain = $"https://{host}{appendPath}";
            }
            return domain;
        }

        public static string AppendQueryString(this string fullUrl, string queryStringName, string queryStringValue)
        {
            var newUrl = fullUrl;
            if (fullUrl.IndexOf("?", StringComparison.Ordinal) >= 0)
            {
                newUrl = $"{newUrl}&{queryStringName}={queryStringValue}";
            }
            else
            {
                newUrl = $"{newUrl}?{queryStringName}={queryStringValue}";
            }
            return newUrl;
        }

        public static string ToUrl(this string url, bool secureLink = false)
        {
            var validUrl = url;
            if (!validUrl.StartsWith("/"))
            {
                validUrl = url.Replace("http://", "").Replace("https://", "");
                var httpCheck = secureLink ? "https://" : "http://";
                if (validUrl.IndexOf(httpCheck, StringComparison.Ordinal) == -1)
                {
                    validUrl = $"{httpCheck}{validUrl}";
                }
            }
            return validUrl;
        }

        public static string ReplaceLineBreaks(this string text, string replaceWith = "")
        {
            var formattedString = text != null ? Regex.Replace(text, @"\r\n?|\n", replaceWith) : null;
            return formattedString;
        }

        public static string RemoveInvalidCharacters(this string originalString)
        {
            var newString = originalString;

            if (!string.IsNullOrEmpty(newString))
            {
                //remove potential angular injection
                newString = newString.Replace("{{", "");
                newString = newString.Replace("}}", "");
                newString = newString.Replace("<", "");
                newString = newString.Replace(">", "");
            }
            else
            {
                newString = string.Empty;
            }
            return newString;
        }

        private static readonly Random Random = new Random();
        public static string RandomAlphaNumericString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static string FormatLargeNumber(double value, string prepend = "")
        {
            var formatValue = $"{value:n0}";

            if (value >= 1000000000)
            {
                //greater than 1 billion
                //formatValue = value.ToString("#,##0,,,.##B", CultureInfo.InvariantCulture);
                formatValue = value.ToString("#,##0,,,.0B", CultureInfo.InvariantCulture);
            }
            else if (value >= 1000000)
            {
                //greater than 1 million
                //formatValue = value.ToString("#,##0,,.##M", CultureInfo.InvariantCulture);
                formatValue = value.ToString("#,##0,,.0M", CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(prepend))
            {
                formatValue = $"{prepend}{formatValue}";
            }

            return formatValue;
        }

        /// <summary>
        /// Trim budge string like "$1.1B" or "$1.1M" to 1100000000 or 1100000
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Raw Value</returns>
        public static double GetBudgeRawValue(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            if (value.EndsWith("B") && value.StartsWith("$"))
            {
                return double.Parse(value.TrimStart('$').TrimEnd('B'))*1000000000;
            }
            if (value.EndsWith("M") && value.StartsWith("$"))
            {
                return double.Parse(value.TrimStart('$').TrimEnd('M'))*1000000;
            }

            return 0;
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

        public static string GetExtensionFromRelativeUrlPath(string url, string validExtensions = "png,jpg,jpeg,png8,gif")
        {
            var extension = url;
            if (extension.IndexOf("?", StringComparison.Ordinal) > -1)
            {
                extension = extension.Substring(0, extension.IndexOf("?", StringComparison.Ordinal));
            }
            if (extension.IndexOf(".", StringComparison.Ordinal) > -1)
            {
                var start = extension.IndexOf(".", StringComparison.Ordinal) + 1;
                var length = extension.Length - extension.IndexOf(".", StringComparison.Ordinal) - 1;
                extension = extension.Substring(start, length);

                var extensionList = validExtensions.Split(',').Select(i => i.Trim().ToLower()).ToList();
                if (!extensionList.Contains(extension))
                {
                    extension = string.Empty;
                }
            }
            else
            {
                extension = string.Empty;
            }
            return extension;
        }

        public static string ReplaceForDeserialization(string jsonData, bool removeWhiteSpaces = true)
        {
            var replacejsonData = jsonData;
            if (removeWhiteSpaces)
            {
                replacejsonData = Regex.Replace(replacejsonData, @"\s+", string.Empty); //remove all white spaces otherwise we have issues with JSON parse in javascript.
            }
            replacejsonData = replacejsonData.Replace("'", "\\'");                    //using single quotes to hold string, so json cannot have single quotes otherwise it will escape.
            return replacejsonData;
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

        public static bool ContainsPhrase(this string searchText, string value)
        {
            var containsValue = false;
            var phraseList = searchText.SmartSplit();
            if (searchText.Contains("\""))
            {
                containsValue = phraseList.Select(i => i.Trim().ToLower()).Contains(value.Trim().ToLower());
            }
            return containsValue;
        }

        /// <summary>
        /// This method is used to ensure no invalid entries are made in search term
        /// Also will trim the word
        /// No odd number of double quotes allowed.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string ValidateSearchTerm(this string word)
        {
            //string myVar = "do this to count the number of words in my wording so that I can word it up!";
            if (string.IsNullOrEmpty(word))
            {
                return word?.Trim();
            }
            word = word.Trim();
            var count = new Regex(Regex.Escape("\"")).Matches(word).Count;
            if (count > 0)
            {
                var remainder = count % 2;
                if (remainder > 0)
                {
                    //odd number of double quotes and is invalid, remove double quotes.
                    word = word.Replace("\"", "").Trim();
                }
            }
            return word;
        }

        public static string RemovePunctuation(this string word)
        {
            var wordRemovePunctuation = word.Replace(".", "").Replace("!", "").Replace(",", "").Replace("?", "").Replace("(", "").Replace(")", "")
                .Replace(";", "").Replace(":", "").Replace("\"", "").Replace("'", "").Replace("<", "").Replace(">", "")
                .Replace("/", "").Replace("\\", "").Replace("[", "").Replace("]", "").Replace("-", "").Replace("+", "").Replace("=", "").Replace("|", "")
                .Replace("{", "").Replace("}", "");
            return wordRemovePunctuation;
        }

        public static string TruncateText(this string fullText, string replaceEnd, int maxLength = 0, bool truncateWord = false)
        {
            string truncatedText = fullText;
            if (maxLength != 0)
            {
                if (fullText.Length > maxLength)
                {
                    truncatedText = fullText.Substring(0, maxLength);
                    if (!truncateWord)
                    {
                        truncatedText = truncatedText.Substring(0, truncatedText.LastIndexOf(" ", StringComparison.Ordinal));
                    }
                    truncatedText += replaceEnd;
                }
            }
            return truncatedText;
        }
    }
}