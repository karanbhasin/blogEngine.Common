using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foundation.Common.Util {
	public static class ErrorUtil {
		public static void ThrowSqlException() {
			System.Data.SqlClient.SqlConnection sqlc = new System.Data.SqlClient.SqlConnection(Foundation.Common.Util.ConfigUtil.GetAppSetting("ChmPortal.databaseConnection"));
			sqlc.Open();
			System.Data.SqlClient.SqlCommand cmd = sqlc.CreateCommand();
			cmd.CommandText = "SELECT * FROM [Table_that_does_not_exist]";
			cmd.CommandTimeout = 2;
			cmd.ExecuteNonQuery();
			cmd.Dispose();
			sqlc.Close();
		}

		public static void ThrowSqlTimeoutException() {
			System.Data.SqlClient.SqlConnection sqlc = new System.Data.SqlClient.SqlConnection(Foundation.Common.Util.ConfigUtil.GetAppSetting("ChmPortal.databaseConnection"));
			sqlc.Open();
			System.Data.SqlClient.SqlCommand cmd = sqlc.CreateCommand();
			cmd.CommandText = "Waitfor delay '00:00:05'";
			cmd.CommandTimeout = 2;
			cmd.ExecuteNonQuery(); // This line will timeout.
			cmd.Dispose();
			sqlc.Close();
		}

		public static void ThrowObjectRef() {
			ErrorUtil.ObjectRef objRef = new ErrorUtil.ObjectRef();
			int[] i = objRef.FailArray;
		}

		public static void ThrowWebException() {
			throw new System.Net.WebException();
		}

		public static void Throw404() {
			System.Web.HttpResponse Response = System.Web.HttpContext.Current.Response;
			Response.Buffer = true;
			Response.StatusCode = 404;
			Response.Status = "404 Page not found";
			Response.End();
		}

		public static void Throw403() {
			System.Web.HttpResponse Response = System.Web.HttpContext.Current.Response;
			Response.Buffer = true;
			Response.StatusCode = 403;
			Response.Status = "403 User not authorized";
			Response.End();
		}

		class ObjectRef {
			System.Collections.Generic.List<int> unInitedList;

			public int[] FailArray {
				get {
					return unInitedList.ToArray();
				}
			}
		}
	}
}
