using System.Text;
using System.Text.RegularExpressions;
namespace System {
    /// <summary>
    /// Provides extension methods for the System.String class.
    /// </summary>
    public static class StringExtensions {
        #region Extension Methods
        /// <summary>
        /// HTML-encodes a string and returns the encoded string.
        /// </summary>
        /// <param name="s">
        /// The text string to encode.
        /// </param>
        /// <returns>
        /// The HTML-encoded text.
        /// </returns>
        public static string HtmlEncode(this string s) {
            return System.Web.HttpUtility.HtmlEncode(s);
        }

        public static string HtmlDecode(this string s) {
            return System.Web.HttpUtility.HtmlDecode(s);
        }

        public static string UrlEncode(this string s) {
            return System.Web.HttpUtility.UrlEncode(s);
        }

        public static string UrlDecode(this string s) {
            return System.Web.HttpUtility.UrlDecode(s);
        }

        public static string StripNonBreakingSpaces(this string s) {
            return s.Replace("&nbsp;", " ");
        }

        /// <summary>
        /// Returns the concatenated form of the string.
        /// <para>`</para>
        /// <para>Example:</para>
        /// <para>string input = "Multiple Option Field";</para>
        /// <para>string output = input.Concatenate(); // The value of output is "MultipleOptionField"</para>
        /// </summary>
        /// <param name="s">
        /// The text string containing text separated by spaces.
        /// </param>
        /// <returns>
        /// The text string with all spaces removed.
        /// </returns>
        public static string Concatenate(this string s) {
            return s.Replace(" ", string.Empty);
        }

        /// <summary>
        /// Returns the string with spaces in front of the capitalized letters.
        /// <para>`</para>
        /// <para>Example:</para>
        /// <para>string input = "MultipleOptionField";</para>
        /// <para>string output = input.ToSpacedName(); // The value of output is "Multiple Option Field"</para>
        /// </summary>
        /// <param name="s">
        /// The text string containing capitalized letters.
        /// </param>
        /// <returns>
        /// The text string with spaces in front of the capitalized letters.
        /// </returns>
        public static string ToSpacedName(this string s) {
            StringBuilder spacedName = new StringBuilder();

            for (int i = 0; i < s.Length; i++) {
                if (i > 0 && i < s.Length - 1 && s.Substring(i, 1).ToUpper() == s.Substring(i, 1)) {
                    spacedName.Append(" ");
                }
                spacedName.Append(s[i]);
            }

            return spacedName.ToString();
        }

        /// <summary>
        /// Returns the plural form of a string.
        /// <para>`</para>
        /// <para>Example:</para>
        /// <para>string input = "church";</para>
        /// <para>string output = input.ToPlural(); // The value of output is "churches"</para>
        /// </summary>
        /// <param name="input">
        /// The text string to make plural.
        /// </param>
        /// <returns>
        /// The text string in plural form.
        /// </returns>
        public static string ToPlural(this string input) {
            string output;

            // TODO: Replace with something more intelligent
            if (input.EndsWith("s") || input.EndsWith("sh") || input.EndsWith("ch") || input.EndsWith("x")) {
                output = input + "es";
            }
            else if (input.EndsWith("y")) {
                output = input.Substring(0, input.Length - 1) + "ies";
            }
            else {
                output = input + "s";
            }

            return output;
        }

		public static string Pluralize(this string s, int count) {
			return Pluralize(s, count, null);
		}

		public static string Pluralize(this string s, int count, string overrideWith) {
			if (String.IsNullOrEmpty(overrideWith)) {
				return count == 1 ? s : s + "s";
			}
			else {
				return count == 1 ? s : overrideWith;

			}
		}

        /// <summary>
        /// Converts a string with underscores to Pascal casing.
        /// <para>`</para>
        /// <para>Example:</para>
        /// <para>string input = "multiple_option_field";</para>
        /// <para>string output = input.ToPascal(); // The value of output is "MultipleOptionField"</para>
        /// </summary>
        /// <param name="s">
        /// The text string to convert to Pascal casing.
        /// </param>
        /// <returns>
        /// The text string in Pascal casing.
        /// </returns>
        public static string ToPascal(this string s) {
            string temp = s;
            if (temp.IndexOf("_", 0, temp.Length) > 0) {
                temp = s.ToLower();

                temp = temp.Substring(0, 1).ToUpper() + temp.Substring(1);
                int uIndex = -1;
                while (temp.IndexOf("_", 0, temp.Length) > 0) {
                    uIndex = temp.IndexOf("_", 0, temp.Length);
                    if (uIndex > 0) {
                        string part1 = temp.Substring(0, uIndex);
                        string part2 = temp.Substring(uIndex + 1);
                        temp.Remove(uIndex, 1);

                        //look for ID at the end of the string and make upper case
                        if (part2 == "id") {
                            part2 = part2.ToUpper();
                        }
                        else {
                            part2 = part2.Substring(0, 1).ToUpper() + part2.Substring(1);
                        }

                        temp = part1 + part2;
                    }
                }
            }
            else {
                temp = temp.Substring(0, 1).ToUpper() + temp.Substring(1);
            }

            return temp;
        }

        /// <summary>
        /// Converts a string with underscores to Camel casing.
        /// <para>`</para>
        /// <para>Example:</para>
        /// <para>string input = "multiple_option_field";</para>
        /// <para>string output = input.ToCamel(); // The value of output is "multipleOptionField"</para>
        /// </summary>
        /// <param name="s">
        /// The text string to convert to Camel casing.
        /// </param>
        /// <returns>
        /// The text string in Camel casing.
        /// </returns>
        public static string ToCamel(this string s) {
            string temp = s.ToPascal();
            return LowerCaseFirstLetter(temp);
        }

        /// <summary>
        /// Returns the possessive form of a string.
        /// <para>`</para>
        /// <para>Example:</para>
        /// <para>string input = "individual";</para>
        /// <para>string output = input.ToPossessive(); // The value of output is "individual's"</para>
        /// </summary>
        /// <param name="s">
        /// The text string to make possessive.
        /// </param>
        /// <returns>
        /// The text string in possessive form.
        /// </returns>
        public static string ToPossessive(this string s) {
            if (s.EndsWith("s") || s.EndsWith("z")) {
                s += "'";
            }
            else {
                s += "'s";
            }

            return s;
        }

        /// <summary>
        /// Determines if a string is empty or contains only whitespace.
        /// </summary>
        /// <param name="s">
        /// The string to check whether it is empty or contains only whitespace.
        /// </param>
        /// <returns>
        /// A boolean determining whether the string is empty or contains only whitespace.
        /// </returns>
        public static bool IsEmpty(this string s) {
            return s == null || Regex.IsMatch(s, @"^\s*$");
        }

        /// <summary>
        /// Compares the source value to a provided value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmpty<T>(this System.Nullable<T> source) where T : struct {

            bool rVal = true;

            if (source.HasValue && !string.IsNullOrEmpty(source.Value.ToString())) {
                rVal = false;
            }

            return rVal;
        }

        public static bool IsNotEmpty(this string s) {
            return !IsEmpty(s);
        }

        public static string Truncate(this string s) {
            return StringExtensions.Truncate(s, 30);
        }

        public static string Truncate(this string s, int length) {
            return StringExtensions.Truncate(s, length, null);
        }

        public static string Truncate(this string s, int length, string suffix) {
            if (String.IsNullOrEmpty(s)) {
                return "";
            }

            suffix = suffix == null ? "..." : suffix;
            return s.Length > length ? s.Substring(0, length - suffix.Length) + suffix : s;
        }

        /// <summary>
        /// Determines if a string matches the specified regular expression.
        /// </summary>
        /// <param name="s">
        /// The input string to match the regular expression against.
        /// </param>
        /// <param name="regularExpression">
        /// The regular expression to match against the input string.
        /// </param>
        /// <returns>
        /// A boolean determining whether the input string matched the regular expression.
        /// </returns>
        public static bool Match(this string s, string regularExpression) {
            // Create the regular expression.
            Regex regex = new Regex(regularExpression);

            // Determine if the specified string matches the regular expression.
            return regex.IsMatch(s.Trim());
        }

        /// <summary>
        /// Converts a string representing a date to a DateTime object.
        /// <para>
        /// Conversion fails if the string does not represent a date.
        /// </para>
        /// </summary>
        /// <param name="s">
        /// The string to check whether it's a valid date.
        /// </param>
        /// <param name="date">
        /// When this method returns, contains the System.DateTime value equivalent to
        ///     the date contained in s, if the conversion succeeded, or System.DateTime.MinValue
        ///     if the conversion failed. The conversion fails if the s parameter is null,
        ///     is an empty string, or does not contain a valid string representation of
        ///     a date.
        /// </param>
        /// <returns>
        /// A boolean determining whether the string is a valid date.
        /// </returns>
        public static bool ConvertToDate(this string s, out DateTime date) {
            bool isValid = false;
            date = DateTime.MinValue;

            //
            // Use the DateTime converter to determine whether the string is a valid date. Since the converter will return true even if the string is a time,
            // verify that the string is not a time by checking whether it contains a foward slash and not a colon.
            //
            if (s.Contains("/") && !s.Contains(":")) {
                isValid = DateTime.TryParse(s, out date);
            }

            return isValid;
        }

        /// <summary>
        /// Converts a string representing a time to a DateTime object.
        /// <para>
        /// Conversion fails if the string does not represent a time.
        /// </para>
        /// </summary>
        /// <param name="s">
        /// The string to check whether it's a valid time.
        /// </param>
        /// <param name="date">
        /// When this method returns, contains the System.DateTime value equivalent to
        ///     the time contained in s, if the conversion succeeded, or System.DateTime.MinValue
        ///     if the conversion failed. The conversion fails if the s parameter is null,
        ///     is an empty string, or does not contain a valid string representation of
        ///     a time.
        /// </param>
        /// <returns>
        /// A boolean determining whether the string is a valid time.
        /// </returns>
        public static bool ConvertToTime(this string s, out DateTime time) {
            bool isValid = false;
            time = DateTime.MinValue;

            //
            // Use the DateTime converter to determine whether the string is a valid time. Since the converter will return true even if the string is a date,
            // verify that the string is not a date by checking whether it contains a colon and not a forward slash.
            //
            if (s.Contains(":") && !s.Contains("/")) {
                isValid = DateTime.TryParse(s, out time);
            }

            return isValid;
        }

        /// <summary>
        /// Converts plain text formatted with Markdown syntax into HTML.
        /// <para>`</para>
        /// <para>Example:</para>
        /// <para>string input = "* bird\n* cat";</para>
        /// <para>string output = "&lt;ul&gt;&lt;li&gt;bird&lt;/li&gt;&lt;li&gt;cat&lt;/li&gt;&lt;/ul&gt;"</para>
        /// </summary>
        /// <param name="s">
        /// The string containing text formatted with Markdown syntax.
        /// </param>
        /// <returns>
        /// The string converted to html and html encoded.
        /// </returns>
        public static string ConvertToHtml(this string s) {
            return OpenMarkdown.OpenMarkdown.ToXhtml(s.HtmlEncode());
        }

		public static bool In(this string s, params string[] values) {
			foreach (string val in values) {
				if (s == val) {
					return true;
				}
			}

			return false;
        }

        /// <summary>
        /// Takes a string and removes any special characters.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string StripSpecialCharacters(this string s) {            
            char[] specials = { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+', '=', '/', '\\', '\'', '"', '<', '>', ',', '.', '?', ':', ';', '-' };
            foreach (char specialChar in specials) {
                s = s.Replace(specialChar.ToString(), "");
            }

            //remove spaces
            s = s.Replace("  ", "_").Replace(" ", "_");

            return s;
        }

		/// <summary>
		/// Masks all or part of a string with a character you specify
		/// ex: acctNum = acctNum.Mask(0, acctNum.Length - 4, '*');  // Show only last 4 digits
		/// </summary>
		/// <param name="s"></param>
		/// <param name="startIndex"></param>
		/// <param name="length"></param>
		/// <param name="maskChar"></param>
		/// <returns></returns>
		public static string Mask(this string s, int startIndex, int length, char maskChar) {
			StringBuilder sb = new StringBuilder(s);

			for (int i = 0; i < length; i++) {
				sb.Replace(sb[i], maskChar, i, 1);
			}

			return sb.ToString();
		}

		/// <summary>
		/// This will escape the quotes in a string with a backslash.
		/// </summary>
		/// <remarks>This is good for taking a name with a quote and using it in a already quoted string.  I.e. the onclick event in HTML.</remarks>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string EscapeQuotes(this string s) {
			return s.Replace("\"", "\\\"").Replace("'", "\\'");
		}

		public static string Capitalize(this string input) {
			if (string.IsNullOrEmpty(input))
				return input;

			return input.Substring(0, 1).ToUpper() + (input.Length == 1 ? string.Empty : input.Substring(1).ToLower());
		}

		public static string ProperCase(this string input) {
			if (string.IsNullOrEmpty(input))
				return input;

			return Regex.Replace(input, "\\b\\w+\\b", match => { return match.Value.Capitalize(); });
		}

		public static string StripHtmlTags(this string input) {
			return Regex.Replace(input, "\\<.*?\\>", String.Empty);
		}

		/// <summary>
		/// Finds all instances of a given string and returns the indexes of each occurrance
		/// </summary>
		/// <param name="search">The string to search for</param>
		/// <returns>An array of ints where each corresponds to the index of the first letter of search string</returns>
		public static int[] FindAll(this string input, string search) {
			System.Collections.ArrayList retArList = new System.Collections.ArrayList();
			try {
				int currentIdx = 0;
				currentIdx = input.IndexOf(search, currentIdx);
				while (currentIdx != -1) {
					retArList.Add(currentIdx);
					currentIdx = input.IndexOf(search, currentIdx + 1);
				}
				return (int[])retArList.ToArray(typeof(int));
			}
			catch {
				return new int[0];
			}
		}

        #region IsEqualTo
        /// <summary>
        /// Compares the source value to a provided value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEqualTo<T>(this System.Nullable<T> source, System.Nullable<T> value) where T : struct {

            bool rVal = false;

            // If both the source and the value have an integer value, capture them and pass to next method
            if (value.HasValue && source.HasValue) {
                rVal = source.Value.IsEqualTo(value.Value);
            }
            else if ((source.HasValue && !value.HasValue) || (!source.HasValue && !value.HasValue)) {
                rVal = false;
            }
            else if (!source.HasValue && !value.HasValue) {
                // They are both null so they are equal
                rVal = true;
            }
            return rVal;
        }

        /// <summary>
        /// Compares the source value to a provided value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEqualTo<T>(this T source, System.Nullable<T> value) where T : struct {

            bool rVal = false;

            if (value.HasValue) {
                T valueNonNull = value.Value;
                rVal = IsEqualTo(source, valueNonNull);
            }

            return rVal;
        }

        /// <summary>
        /// Compares the source value to a provided value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEqualTo<T>(this System.Nullable<T> source, T value) where T : struct {

            bool rVal = false;

            if (source.HasValue) {
                rVal = source.Value.IsEqualTo(value);
            }

            return rVal;
        }

        /// <summary>
        /// Compares the source value to a provided value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEqualTo<T>(this T source, T value) where T : struct {
            return source.Equals(value);
        }

        public static bool IsEqualTo<T>(this string source, System.Nullable<T> value) where T : struct {

            bool rVal = false;

            if (value.HasValue) {
                rVal = source.IsEqualTo(value.Value);
            }
            else {
                rVal = source.Equals(value);
            }

            return rVal;
        }

        /// <summary>
        /// Compares the source value to a provided value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEqualTo<T>(this string source, T value) {

            if (!string.IsNullOrEmpty(source)) {
                return source.Equals(value.ToString());
            }
            else {
                // If the source is null, determine if the value is null
                // if the value is null, the two are equal
                return string.IsNullOrEmpty(value.ToString());
            }
        }

        #endregion IsEqualTo

        #region Conversions
        public static int IntegerValue(this string s) {

            int returnValue = int.MinValue;
            int.TryParse(s, out returnValue);
            return returnValue;
        }
        #endregion Conversions
        #endregion

        #region Helpers
        /// <summary>
        /// Returns a string with the first letter in lowercase.
        /// </summary>
        /// <param name="s">
        /// The text string.
        /// </param>
        /// <returns>
        /// The text string with the first letter in lowercase.
        /// </returns>
        private static string LowerCaseFirstLetter(string s) {
            return s.Substring(0, 1).ToLower() + s.Substring(1);
        }
        #endregion
    }
}
