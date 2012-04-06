using System;
using System.Resources;

namespace Foundation.Common.Util
{
	/// <summary>
	/// Summary description for ResourceUtil.
	/// </summary>
	public class ResourceUtil
	{
		#region Private Properties
		private static System.Resources.ResourceManager _RM = null;
		#endregion Private Properties

		#region Public Properties
		public static System.Resources.ResourceManager RM {
			set { _RM = value; }
		}
		#endregion Public Properties

		#region Constructors
		private ResourceUtil()
		{
		}
		#endregion Constructors

		#region Public Methods
		public static string GetString(string sKey) {
			return _RM.GetString(sKey);
		}
		#endregion Public Methods
		
	}
}
