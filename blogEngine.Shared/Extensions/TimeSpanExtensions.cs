using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System {
    public static class TimeSpanExtensions {
        public static string ToString(this TimeSpan ts, string format) {
            DateTime dt = new DateTime(ts.Ticks);
            return dt.ToString(format);
        }

        public static DateTime? DateTime(this TimeSpan? ts) {
            if (ts == null)
                return null;
            else
                return new DateTime(ts.Value.Ticks);
        }
    }

    public static class DateUtil {
        public static int GetLastDay(int year, int month) {
            switch (month) {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    return 31;
                case 4:
                case 6:
                case 9:
                case 11:
                    return 30;
                default:
                    return DateTime.IsLeapYear(year) ? 29 : 28;

            }
        }
    }
}
