using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Text;

namespace Foundation.Common.Util {
	
	public class ConvertUtil{
		#region Conversion Methods
		public static byte[] ConvertToByteArray(object value) {
			byte[] result = null;

			if (!Convert.IsDBNull(value)) {
				result = (byte[])value;
			}

			return result;
		}

		public static byte ConvertToByte(object value) {
			byte result = NullUtil.ByteNull;

			if (!Convert.IsDBNull(value)) {
				result = (byte)value;
			}

			return result;
		}

		public static string ConvertToString(object value) {
			string result = null;

			if (!Convert.IsDBNull(value)) {
				result = Convert.ToString(value);
			}

			return result;
		}

		public static int ConvertToInt32(object value) {
			int result = NullUtil.Int32Null;

			if (!Convert.IsDBNull(value)) {
				result = Convert.ToInt32(value);
			}

			return result;
		}

        public static Int64 ConvertToInt64(object value)
        {
            long result = NullUtil.Int64Null;

            if (!Convert.IsDBNull(value))
            {
                result = Convert.ToInt64(value);
            }

            return result;
        }

        public static Int16 ConvertToInt16(object value)
        {
            Int16 result = NullUtil.Int16Null;

            if (!Convert.IsDBNull(value))
            {
                result = Convert.ToInt16(value);
            }

            return result;
        }

		public static double ConvertToDouble(object value) {
			double result = NullUtil.DoubleNull;

			if (!Convert.IsDBNull(value)) {
				result = Convert.ToDouble(value);
			}

			return result;
		}

		public static decimal ConvertToDecimal(object value) {
			decimal result = NullUtil.DecimalNull;

			if (!Convert.IsDBNull(value)) {
				result = Convert.ToDecimal(value);
			}

			return result;
		}

		public static DateTime ConvertToDateTime(object value) {
			DateTime result = NullUtil.DateTimeNull;

			if (!Convert.IsDBNull(value)) {
				result = Convert.ToDateTime(value);
			}

			return result;
		}

		public static bool ConvertToBoolean(object value) {
			bool result = false;

			if (!Convert.IsDBNull(value)) {
				result = Convert.ToBoolean(value);
			}

			return result;
		}

        public static string ConvertToDelimited(ArrayList values, string delimiter) {
			string rVal = null;
			if(values != null && values.Count > 0) {				
				switch(values[0].GetType().ToString()){
					case "System.String":
						rVal = ConvertToDelimited(values, delimiter, SqlDbType.VarChar);
						break;
					default:
						rVal = ConvertToDelimited(values, delimiter, SqlDbType.Int);
						break;
				}								
			}
			return rVal;
		}

		public static string ConvertToDelimited(ArrayList values, string delimiter, System.Data.SqlDbType sqlDbType) {
			StringBuilder sqlToAppend = new StringBuilder();
			if(values != null && values.Count > 0) {
				switch(sqlDbType){ 
					case SqlDbType.VarChar:
						for(int i = 0; i < values.Count; i++){
							if(i == 0) {
								sqlToAppend.Append("'" + values[i].ToString().Replace("'", "''") + "'");
							}
							else {
								sqlToAppend.Append(delimiter + "'" + values[i].ToString().Replace("'", "''") + "'");
							}
						}
						break;
					default:
						for(int i = 0; i < values.Count; i++){
							if(i == 0) {
								sqlToAppend.Append(values[i].ToString());
							}
							else {
								sqlToAppend.Append(delimiter + values[i].ToString());
							}
						}
						break;
				}
			}
			else {
				return null;
			}
			return sqlToAppend.ToString();
		}
		
		#endregion
	}




	/// <summary>
	/// Handles the representation and evaluation of null values for 
	/// primitives that natively do not support null values.
	/// </summary>
	public class NullUtil {

        public const Int16 Int16Null = Int16.MinValue;
		/// <summary>
		/// Constant that represents a null value for an int.
		/// </summary>
		public const int Int32Null = Int32.MinValue;

        public const Int64 Int64Null = Int64.MinValue;

		/// <summary>
		/// Constant that represents a null value for a double.
		/// </summary>
		public const double DoubleNull = Double.MinValue;

		/// <summary>
		/// Constant that represents a null value for a DateTime.
		/// </summary>
		public static DateTime DateTimeNull = DateTime.MinValue;

		/// <summary>
		/// Constant that represents a null value for a Decimal.
		/// </summary>
		public static Decimal DecimalNull = Decimal.MinValue;

		/// <summary>
		/// Constant that represents a null value for a Byte.
		/// </summary>
		public static Byte ByteNull = Byte.MinValue;

		/// <summary>
		/// Constant that represents a null value for a Guid.
		/// </summary>
		public static Guid GuidNull = Guid.Empty;

		/// <summary>
		/// Evaluates whether an int value is null.
		/// </summary>
		/// <param name="value">The int value to evaluate</param>
		/// <returns>A bool indicating whether the value is null</returns>
		public static bool IsNull(int value) {
			if (value == Int32Null) {
				return true;
			}
			else {
				return false;
			}
		}

        /// <summary>
        /// Evaluates whether an int value is null.
        /// </summary>
        /// <param name="value">The int value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(int? value) {
            if (value == null || value == Int32Null) {
                return true;
            } else {
                return false;
            }
        }

		/// <summary>
		/// Evaluates whether a nullable boolean is null.
		/// </summary>
		/// <param name="value">The nullable boolean to evaluate</param>
		/// <returns>A bool indicating whether the value is null</returns>
		public static bool IsNull(bool? value) {
			if (value == null) {
				return true;
			}
			else {
				return false;
			}
		}
		
		/// <summary>
		/// Evaluates whether a double value is null.
		/// </summary>
		/// <param name="value">The double value to evaluate</param>
		/// <returns>A bool indicating whether the value is null</returns>
		public static bool IsNull(double value) {
			if (value == DoubleNull) {
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// Evaluates whether a DateTime value is null.
		/// </summary>
		/// <param name="value">The DateTime value to evaluate</param>
		/// <returns>A bool indicating whether the value is null</returns>
		public static bool IsNull(DateTime value) {
			DateTime sqlMinDateTime = new DateTime(1753, 1, 1);

			if (value < sqlMinDateTime) {
				return true;
			}
			else {
				return false;
			}
		}

        /// <summary>
        /// Evaluates whether a DateTime value is null.
        /// </summary>
        /// <param name="value">The DateTime value to evaluate</param>
        /// <returns>A bool indicating whether the value is null</returns>
        public static bool IsNull(DateTime? value) {
            if (value == null) {
                return true;
            }
            else {
                return IsNull(value.Value);
            }
        }

		/// <summary>
		/// Evaluates whether a string value is null.
		/// </summary>
		/// <param name="value">The string value to evaluate</param>
		/// <returns>A bool indicating whether the value is null</returns>
		public static bool IsNull(string value) {
			if (value != null) {
				value = value.Trim();
			}

			if (value == null || value == string.Empty) {
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// Evaluates whether a decimal value is null.
		/// </summary>
		/// <param name="value">The decimal value to evaluate</param>
		/// <returns>A bool indicating whether the value is null</returns>
		public static bool IsNull(decimal value) {
			if (value == DecimalNull || value == Int32Null) {
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// Evaluates whether a decimal value is null.
		/// </summary>
		/// <param name="value">The decimal value to evaluate</param>
		/// <returns>A bool indicating whether the value is null</returns>
		public static bool IsNull(decimal? value) {

            bool rVal = true;

            if (value.HasValue) {
                rVal = IsNull(value.Value);
            }

			return rVal;
		}

		/// <summary>
		/// Evaluates whether a Guid value is null.
		/// </summary>
		/// <param name="value">The Guid value to evaluate</param>
		/// <returns>A bool indicating whether the value is null</returns>
		public static bool IsNull(Guid value) {
			if (value == GuidNull) {
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// Evaluates whether a Guid value is null.
		/// </summary>
		/// <param name="value">The Guid value to evaluate</param>
		/// <returns>A bool indicating whether the value is null</returns>
		public static bool IsNull(Guid? value) {
			if (value == null) {
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// Evaluates whether an enum value is null.
		/// </summary>
		/// <param name="value">The enum value to evaluate</param>
		/// <returns>A bool indicating whether the value is null</returns>
		public static bool IsNull(System.Enum value) {
			
			if (value == null || !System.Enum.IsDefined(value.GetType(), value)) {
				return true;
			}
			else {
				return false;
			}
		}
		
	}

    public static class FTConvert {
        public static int ToInt32(string parm) {
            return string.IsNullOrEmpty(parm) ? int.MinValue : int.Parse(parm);
        }

        public static bool ToBoolean(string parm) {
            return string.IsNullOrEmpty(parm) ? false : bool.Parse(parm);
        }

        public static DateTime ToDateTime(string parm) {
            return string.IsNullOrEmpty(parm) ? DateTime.MinValue : DateTime.Parse(parm);
        }
    }
}
