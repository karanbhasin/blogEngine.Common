using System;
using System.Collections.Generic;
using System.Text;

namespace Foundation.Common.Util {
    [Serializable()]
    public class StringUtil {
        public static string Truncate(string value) {
            return StringUtil.Truncate(value, 30);
        }

        public static string Truncate(string value, int length) {
            return StringUtil.Truncate(value, length, null);
        }

        public static string Truncate(string value, int length, string suffix) {
			if (String.IsNullOrEmpty(value)) {
				return "";
			}

            suffix = suffix == null ? "..." : suffix;
            return value.Length > length ? value.Substring(0, length - suffix.Length) + suffix : value;
        }

		public static string MakePossessive(string name) {
			string possessive = name;

			if (name.EndsWith("s")) {
				possessive += "\\'";
			}
			else {
				possessive += "\\'s";
			}

			return possessive;
		}
    }
}
