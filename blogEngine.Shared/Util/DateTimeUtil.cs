using System;
using System.Data;
using System.Globalization;
using System.Web.Caching;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Configuration;
using System.Threading;

namespace Foundation.Common.Util {

	[Serializable]
	public enum DateType {
		Minutes,
		Seconds,
		Ticks,
		Milliseconds,
		Years,
		TwiceYears,
		Quarters,
		Months,
		TwiceMonths,
		Weeks,
		Days
	}

	[Serializable]
	public enum DateTimeFormatType {
		LongDatePattern,
		LongDateTimePattern,
		ShortDatePattern,
		ShortDateTimePattern,
		LongTimePattern,
		ShortTimePattern,
		MonthDayPattern,
		YearMonthPattern,
		FullDateTimePattern,
		RFC1123Pattern,
		SortableDateTimePattern,
		UniversalSortableDateTimePattern
	}
    
    /// <summary>
    /// The Timespan Type that can be stored with a date range so 
    /// that the range (as represented by an integer) can be properly interpreted
    /// </summary>
    [Serializable]
    public enum DateRangeType {
        Unspecified = 0,
        Ticks = 1,
        Milliseconds = 2,
        Seconds = 3,
        Days = 4,
        Weeks = 5,
        Months = 6,
        Years = 7
    }
	
	[Serializable]
	public class Date{
		/// <summary>
		/// DateDiff
		/// </summary>
		/// <param name="dateType"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns>Decimal based on ticks</returns>
		public decimal DateDiff(Foundation.Common.Util.DateType dateType, System.DateTime startDate, System.DateTime endDate) { 
			decimal diff = 0; 
			try { 
				System.TimeSpan TS = new System.TimeSpan(endDate.Ticks-startDate.Ticks); 
				switch (dateType) { 
					case DateType.Minutes: //"minutes": 
						diff =  Convert.ToDecimal(TS.TotalMinutes); 
						break; 
					case DateType.Seconds: //"seconds": 
						diff = Convert.ToDecimal(TS.TotalSeconds); 
						break; 
					case DateType.Ticks: //"ticks": 
						diff = Convert.ToDecimal(TS.Ticks); 
						break; 
					case DateType.Milliseconds: //"milliseconds": 
						diff = Convert.ToDecimal(TS.TotalMilliseconds); 
						break; 
					case DateType.Years: //"years": 
						diff = Convert.ToDecimal(TS.TotalDays/365); 
						break; 
					case DateType.TwiceYears: //"years": 
						diff = Convert.ToDecimal(TS.TotalDays/182.50); 
						break; 
					case DateType.Quarters: //"quarters": 
						diff = Convert.ToDecimal((TS.TotalDays/365) * 4); 
						break;
					case DateType.Months: //"months": 
						diff = Convert.ToDecimal((TS.TotalDays/365) * 12); 
						break;
					case DateType.TwiceMonths: //"months": 
						diff = Convert.ToDecimal((TS.TotalDays/365) * 24); 
						break;
					case DateType.Weeks: //"weeks": 
						diff = Convert.ToDecimal((TS.TotalDays/365)* 52); 
						break; 
					case DateType.Days: //"days": 
						diff = Convert.ToDecimal(TS.TotalDays); 
						break; 
					default: //d 
						diff = Convert.ToDecimal(TS.TotalDays); 
						break; 
				} 
			} 
			catch { 
				diff = -1; 
			} 
			return diff; 
		}
	
		
		/// <summary>
		/// "ex.""mm/dd/yyyy"" (specify the disired format be using Foundation.Common.Util.DateTimeFormatType) 
		///parameters
		///	- qualifierPosition is used then the following sting is returned:"{0:mm/dd/yyyy}" or "{1:mm/dd/yyyy}" etc... 
		///	depending on the qualifierPostition 	(returns string)
		/// </summary>
		/// <param name="dateTimeFormatType"></param>
		/// <param name="qualifierPosition"></param>
		/// <returns></returns>
		public static string DateTimeFormat(DateTimeFormatType dateTimeFormatType, int qualifierPosition){
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if (qualifierPosition != Foundation.Common.Util.NullUtil.Int32Null) {
				sb.Append("{");
				sb.Append(qualifierPosition);
				sb.Append(":");
			}
			switch(dateTimeFormatType){
				case DateTimeFormatType.LongDatePattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongDatePattern);
					break;
				case DateTimeFormatType.LongDateTimePattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongDatePattern);
					sb.Append(" ");
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongTimePattern);
					break;
				case DateTimeFormatType.LongTimePattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongTimePattern);
					break;
				case DateTimeFormatType.ShortDatePattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern);
					break;
				case DateTimeFormatType.ShortDateTimePattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern);
					sb.Append(" ");
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortTimePattern);
					break;
				case DateTimeFormatType.ShortTimePattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortTimePattern);
					break;
				case DateTimeFormatType.MonthDayPattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.MonthDayPattern);
					break;
				case DateTimeFormatType.YearMonthPattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.YearMonthPattern);
					break;
				case DateTimeFormatType.FullDateTimePattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.FullDateTimePattern);
					break;
				case DateTimeFormatType.RFC1123Pattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.RFC1123Pattern);
					break;
				case DateTimeFormatType.SortableDateTimePattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.SortableDateTimePattern);
					break;
				case DateTimeFormatType.UniversalSortableDateTimePattern:
					sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.UniversalSortableDateTimePattern);
					break;
			
			}
			if (qualifierPosition != Foundation.Common.Util.NullUtil.Int32Null) {
				sb.Append("}");
			}
			return sb.ToString();
		}

		/// <summary>
		/// "ex.""mm/dd/yyyy"" (specify the disired format be using Foundation.Common.Util.DateTimeFormatType)
		///parameters 
		///	- qualifierPosition: the following sting is returned id > NullUtil.IntMinvalue:""{0:mm/dd/yyyy}"" or ""{1:mm/dd/yyyy}"" etc... depending on the qualifierPostition 
		///- CultureInfo: Implied culture object that, when passed, will alter the default culture's DateTime Patterns while retreaving the format. 
		///(returns string)"
		/// </summary>
		/// <param name="dateTimeFormatType"></param>
		/// <param name="cultureInfo"></param>
		/// <param name="qualifierPosition"></param>
		/// <returns></returns>
		public static string DateTimeFormat(DateTimeFormatType dateTimeFormatType, System.Globalization.CultureInfo cultureInfo, int qualifierPosition){
				
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if (qualifierPosition != Foundation.Common.Util.NullUtil.Int32Null) {
				sb.Append("{");
				sb.Append(qualifierPosition);
				sb.Append(":");
			}
			switch (dateTimeFormatType) {
			case DateTimeFormatType.LongDatePattern:
				sb.Append(cultureInfo.DateTimeFormat.LongDatePattern);
				break;
			case DateTimeFormatType.LongDateTimePattern:
				sb.Append(cultureInfo.DateTimeFormat.LongDatePattern);
				sb.Append(" ");
				sb.Append(cultureInfo.DateTimeFormat.LongTimePattern);
				break;
			case DateTimeFormatType.LongTimePattern:
				sb.Append(cultureInfo.DateTimeFormat.LongTimePattern);
				break;
			case DateTimeFormatType.ShortDatePattern:
				sb.Append(cultureInfo.DateTimeFormat.ShortDatePattern);
				break;
			case DateTimeFormatType.ShortDateTimePattern:
				sb.Append(cultureInfo.DateTimeFormat.ShortDatePattern);
				sb.Append(" ");
				sb.Append(cultureInfo.DateTimeFormat.ShortTimePattern);
				break;
			case DateTimeFormatType.ShortTimePattern:
				sb.Append(cultureInfo.DateTimeFormat.ShortTimePattern);
				break;
			case DateTimeFormatType.MonthDayPattern:
				sb.Append(cultureInfo.DateTimeFormat.MonthDayPattern);
				break;
			case DateTimeFormatType.YearMonthPattern:
				sb.Append(cultureInfo.DateTimeFormat.YearMonthPattern);
				break;
			case DateTimeFormatType.FullDateTimePattern:
				sb.Append(cultureInfo.DateTimeFormat.FullDateTimePattern);
				break;
			case DateTimeFormatType.RFC1123Pattern:
				sb.Append(cultureInfo.DateTimeFormat.RFC1123Pattern);
				break;
			case DateTimeFormatType.SortableDateTimePattern:
				sb.Append(cultureInfo.DateTimeFormat.SortableDateTimePattern);
				break;
			case DateTimeFormatType.UniversalSortableDateTimePattern:
				sb.Append(cultureInfo.DateTimeFormat.UniversalSortableDateTimePattern);
				break;
			
			}
			if (qualifierPosition != Foundation.Common.Util.NullUtil.Int32Null) {
				sb.Append("}");
			}
			return sb.ToString();
		}

		/// <summary>
		/// ToString(System.DateTime sourceDate, DateTimeFormatType dateTimeFormatType)
		///parameters 
		///	- sourceDate: the date to be formatted 
		///- dateTimeFormatType: the format to apply - this will drive from the current thread
		///(returns string)"
		/// </summary>
		/// <param name="dateTimeFormatType"></param>
		/// <param name="cultureInfo"></param>
		/// <param name="qualifierPosition"></param>
		/// <returns></returns>
		public static string ToString(System.DateTime sourceDate, DateTimeFormatType dateTimeFormatType){
				
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			switch(dateTimeFormatType){
			case DateTimeFormatType.LongDatePattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongDatePattern);
				break;
			case DateTimeFormatType.LongDateTimePattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongDatePattern);
				sb.Append(" ");
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongTimePattern);
				break;
			case DateTimeFormatType.LongTimePattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongTimePattern);
				break;
			case DateTimeFormatType.ShortDatePattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern);
				break;
			case DateTimeFormatType.ShortDateTimePattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern);
				sb.Append(" ");
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortTimePattern);
				break;
			case DateTimeFormatType.ShortTimePattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortTimePattern);
				break;
			case DateTimeFormatType.MonthDayPattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.MonthDayPattern);
				break;
			case DateTimeFormatType.YearMonthPattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.YearMonthPattern);
				break;
			case DateTimeFormatType.FullDateTimePattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.FullDateTimePattern);
				break;
			case DateTimeFormatType.RFC1123Pattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.RFC1123Pattern);
				break;
			case DateTimeFormatType.SortableDateTimePattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.SortableDateTimePattern);
				break;
			case DateTimeFormatType.UniversalSortableDateTimePattern:
				sb.Append(Thread.CurrentThread.CurrentUICulture.DateTimeFormat.UniversalSortableDateTimePattern);
				break;
			}

			return sourceDate.ToString(sb.ToString());
		}

        /// <summary>
        /// "Converts a DateTime to an invariant format(Using the Patterns from the Invariant culture type)
        ///parameters
        ///	- sourceDate DateTime in any culture specific format
        /// Translates date to string formatted as: ""yyyy-MM-dd HH:m:ss.fff""
        ///(returns string)"
        /// </summary>
        /// <param name="sourceDate"></param>
        /// <returns></returns>
        public static string TemporaryConvertToInvariant(DateTime sourceDate)
        {

            string cultureName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            string convertedDate;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            //convertedDate = sourceDate.ToString("yyyy-MM-dd HH:m:ss.fff");
            convertedDate = sourceDate.ToString("yyyy-MM-dd");
            //convertedDate = sourceDate.ToString("yyyy-MM-ddTHH:m:ss");

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(cultureName);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            return convertedDate;

        }		


		/// <summary>
		/// "Converts a DateTime to an invariant format(Using the Patterns from the Invariant culture type)
		///parameters
		///	- sourceDate DateTime in any culture specific format
		/// Translates date to string formatted as: ""yyyy-MM-dd HH:m:ss.fff""
		///(returns string)"
		/// </summary>
		/// <param name="sourceDate"></param>
		/// <returns></returns>
		public static string ConvertToInvariant(DateTime sourceDate){
		
			string cultureName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
			string convertedDate;
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			
			convertedDate = sourceDate.ToString("yyyy-MM-dd HH:m:ss.fff");
			
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(cultureName);
			System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			
			return convertedDate;
			
		}
        /// <summary>
        /// "Converts a DateTime to an invariant format(Using the Patterns from the Invariant culture type)
        ///parameters
        ///	- sourceDate DateTime in any culture specific format
        /// Translates date to string formatted as: ""yyyy-MM-dd HH:m:ss.fff""
        ///(returns string)"
        /// </summary>
        /// <param name="sourceDate"></param>
        /// <returns></returns>
        public static string ConvertToInvariant(DateTime? sourceDate) {

            string cultureName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            string convertedDate;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            convertedDate = ((DateTime)sourceDate).ToString("yyyy-MM-dd HH:m:ss.fff");

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(cultureName);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            return convertedDate;

        }
		/// <summary>
		/// "Converts a DateTime to an invariant format(Using the Patterns from the Invariant culture type)
		///parameters
		///- sourceDate DateTime in any culture specific format
		///- DateTimeFormatType Format type to use when translating to Invariant
		///Translates date to string with format specified
		///(returns string)"
		/// </summary>
		/// <param name="sourceDate"></param>
		/// <param name="dateTimeFormatType"></param>
		/// <returns></returns>
		public static string ConvertToInvariant(DateTime sourceDate, DateTimeFormatType dateTimeFormatType){
		
			string cultureName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
			string convertedDate;
			string format = string.Empty;
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			
			switch(dateTimeFormatType){
				case DateTimeFormatType.LongDatePattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongDatePattern;
					break;
				case DateTimeFormatType.LongDateTimePattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongDatePattern + " " + Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongTimePattern;
					break;
				case DateTimeFormatType.LongTimePattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongTimePattern;
					break;
				case DateTimeFormatType.ShortDatePattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern;
					break;
				case DateTimeFormatType.ShortDateTimePattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern + " " + Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongTimePattern;
					break;
				case DateTimeFormatType.ShortTimePattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortTimePattern;
					break;
				case DateTimeFormatType.MonthDayPattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.MonthDayPattern;
					break;
				case DateTimeFormatType.YearMonthPattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.YearMonthPattern;
					break;
				case DateTimeFormatType.FullDateTimePattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.FullDateTimePattern;
					break;
				case DateTimeFormatType.RFC1123Pattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.RFC1123Pattern;
					break;
				case DateTimeFormatType.SortableDateTimePattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.SortableDateTimePattern;
					break;
				case DateTimeFormatType.UniversalSortableDateTimePattern:
					format = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.UniversalSortableDateTimePattern;
					break;
			
			}
			convertedDate = sourceDate.ToString(format);
			
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(cultureName);
			System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			
			return convertedDate;
			
		}

		/// <summary>
		/// Parse
		/// </summary>
		/// <param name="sourceDateString"></param>
		/// <param name="cultureInfo">
		/// The cultureInfo using the following syntax: 
		/// For current configured culture: System.Threading.Thread.CurrentThread.CurrentCulture
		/// For server based configured culture: System.Globalization.CultureInfo.InvariantCulture
		/// </param>
		/// <returns>A DateTime based on the provided culture</returns>
		public static DateTime Parse(string sourceDateString){
			return Parse(sourceDateString, System.Globalization.CultureInfo.InvariantCulture);
		}
		
		/// <summary>
		/// Parse
		/// </summary>
		/// <param name="sourceDateString"></param>
		/// <param name="cultureInfo">
		/// The cultureInfo using the following syntax: 
		/// For current configured culture: System.Threading.Thread.CurrentThread.CurrentCulture
		/// For server based configured culture: System.Globalization.CultureInfo.InvariantCulture
		/// </param>
		/// <returns>A DateTime based on the provided culture</returns>
		public static DateTime Parse(string sourceDateString, System.Globalization.CultureInfo cultureInfo){
			
			DateTime convertedDate;
			if(cultureInfo == null){
				cultureInfo = System.Globalization.CultureInfo.InvariantCulture;			
			}
            convertedDate = DateTime.Parse(sourceDateString,cultureInfo);
			
			return convertedDate;
			
		}
		/// <summary>
		/// "Converts a string to a culture specific formatted DateTime
		///parameters 
		///- sourceDateString string date in any culture specific format
		///- cultureInfo (System.Globalization.CultureInfo) Specified culture that the date will be parsed with
		///Note: This translates a base or invariant date into a date that fits the culture specifed
		///(returns DateTime)"
		/// </summary>
		/// <param name="sourceDateString"></param>
		/// <param name="cultureInfo"></param>
		/// <returns></returns>
		public static DateTime ParseToCurrentCulture(string sourceDateString, System.Globalization.CultureInfo cultureInfo){
			
			DateTime convertedDate;
			string stringDate;
			if(cultureInfo == null){
				cultureInfo = System.Globalization.CultureInfo.InvariantCulture;			
			}

			convertedDate = DateTime.Parse(sourceDateString,System.Globalization.CultureInfo.InvariantCulture);
			stringDate = convertedDate.ToString(cultureInfo.DateTimeFormat.ShortDatePattern + " " + cultureInfo.DateTimeFormat.LongTimePattern);
			convertedDate = DateTime.Parse(stringDate,cultureInfo);
			
			return convertedDate;
			
		}
    }
	

	/// <summary>
	/// A structure (similiar to the DateTime structure) that is used for 
	/// performing calculations of the differences it time between various
	/// time zones.  This is the equivalent of the SYSTEMTIME structure 
	/// in the Windows API.
	/// </summary>	
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct SystemTime {
		public short Year;
		public short Month;
		public short DayOfWeek;
		public short Day;
		public short Hour;
		public short Minute;
		public short Second;
		public short Millisecond;

		#region external Methods
		[DllImport("kernel32.dll", ExactSpelling=true)]
		internal static extern void GetLocalTime(out SystemTime localTime);
		[DllImport("kernel32.dll", ExactSpelling=true)]
		internal static extern void GetSystemTime(out SystemTime localTime);
		#endregion

		#region public instance methods
		/// <summary>
		/// Converts the SystemTime structure to a System.DateTime structure
		/// </summary>
		public DateTime ToDateTime(){
			//construct a DateTime object from the SystemTime object that was passed in
			return SystemTime.ToDateTime(this);
		}
		
		#endregion

		#region static conversion Methods
		/// <summary>
		/// Converts a SystemTime structure to a System.DateTime structure
		/// </summary>
		/// <param name="time">The SystemTime structure to be converted</param>
		/// <returns>A System.DateTime structure that was created based on
		/// the value of the SystemTime structure.</returns>
		public static DateTime ToDateTime(SystemTime time){
			//construct a DateTime object from the SystemTime object that was passed in
			return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
		}
		/// <summary>
		/// Converts a System.DateTime structure to a SystemTime structure
		/// </summary>
		/// <param name="time">System.DateTime structure to be converted</param>
		/// <returns>A SystemTime structure that was created based on
		/// the value of the System.DateTime structure.</returns>
		public static SystemTime FromDateTime(DateTime time){ 
			//create the SystemTime object and populate it's values from the 
			//DateTime object that was passed in
			SystemTime sysTime;
			sysTime.Day = (short)time.Day;
			sysTime.DayOfWeek = (short)time.DayOfWeek;
			sysTime.Hour = (short)time.Hour;
			sysTime.Millisecond = (short)time.Millisecond;
			sysTime.Minute = (short)time.Minute;
			sysTime.Month = (short)time.Month;
			sysTime.Second = (short)time.Second;
			sysTime.Year = (short)time.Year;
			return sysTime;
		}

		#endregion
	}

	[Serializable]
	public class TimeUtil {
		private TimeUtil() {}

		/// <summary>
		/// Converts a friendly time string to minutes.
		/// </summary>
		/// <param name="time">Format expected:  1:30 represents 1hr 30min</param>
		/// <returns>Time in minutes</returns>
		public static int ConvertToMinutes(string time) {
			int minutes = 0;
			if (! Foundation.Common.Util.NullUtil.IsNull(time)) {
				if (time.IndexOf(":") > -1) {
					string[] parts = time.Split(':');
					if (!Foundation.Common.Util.NullUtil.IsNull(parts[0])) {
						//hour
						minutes = Int32.Parse(parts[0]) * 60;
					}

					if (!Foundation.Common.Util.NullUtil.IsNull(parts[1])) {
						//minutes
						minutes += Int32.Parse(parts[1]);
					}
				}
				else {
					minutes = Int32.Parse(time) * 60;
				}
			}

			if (minutes == 0) {
				minutes = NullUtil.Int32Null;
			}

			return minutes;
		}
	}
}
