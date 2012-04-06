using System;
using System.Text;

namespace System {
	public static class IntegerExtensions {
		/// <summary>
		/// This takes an integer and returns an ordinal suffixed string (I.e. 1st, 2nd, etc.)
		/// </summary>
		/// <param name="number">The integer object (extending Int32)</param>
		/// <returns>A string with the number suffixed by an ordinal</returns>
		public static string ToOrdinalString(this int number) {
			if (number == 0) {
				return number.ToString();
			}

			String retNumber = number.ToString();

			switch (number % 100) {
				case 11:
				case 12:
				case 13:
					return retNumber + "th";
			}

			switch (number % 10) {
				case 1: // 1st
					return retNumber + "st";
				case 2: // 2nd
					return retNumber + "nd";
				case 3: // 3rd
					return retNumber + "rd";
				default: // n-th
					return retNumber + "th";
			}
		}

        /// <summary>This takes an integer and returns a string with commas for the thousands</summary>
        /// <param name="number">The integer object (extending Int32)</param>
        /// <returns>A string with the commas</returns>
        public static string ToStringWithCommas(this int number) {
            if (number == 0) return "0";            
            return string.Format("{0:#,#}", number);
        }

        /// <summary>This takes an nullable integer and returns a string with commas for the thousands</summary>
        /// <param name="number">The nullable integer object (extending Int32)</param>
        /// <returns>A string with the commas</returns>
        public static string ToStringWithCommas(this int? number) {
            if (!number.HasValue) return "0";
            return number.Value.ToStringWithCommas();
        }
	}
}
