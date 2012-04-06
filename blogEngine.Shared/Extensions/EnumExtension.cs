using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;

namespace System {
	public static class EnumExtension {
		public static string ToLocalizedString(this System.Enum e) {
			StringBuilder resourceFile = new StringBuilder();
			string localizedDescription = e.ToString();

			//Format: [AssemblyName].Resources.[CurrentUICulture].Enum.[GetType().Name]
			//ex. FellowshipTech.Model.Resources.es.Enum.ActiveStatus
			resourceFile.Append(e.GetType().Assembly.GetName().Name).Append(".Resources.Enum.").Append(e.GetType().Name);
			ResourceManager _resources = new ResourceManager(resourceFile.ToString(), e.GetType().Assembly);

			string rk = String.Format("{0}.{1}", e.GetType(), e);

			if (_resources.GetString(rk) != null) {
				localizedDescription = _resources.GetString(rk);
			}
			return localizedDescription;
		}
	}
	///Not sure why, but the code below will not find any manafest resource names if the name has a . in it i.e. ActiveStatus.es.resx gets left out
	////public static class EnumExtension {
	////    public static string ToLocalizedString(this System.Enum e) {
	////        StringBuilder resourceFile = new StringBuilder();
	////        string localizedDescription = e.ToString();

	////        //Format: [AssemblyName].Resources.[CurrentUICulture].Enum.[GetType().Name]
	////        //ex. FellowshipTech.Model.Resources.es.Enum.ActiveStatus
	////        resourceFile.Append(e.GetType().Assembly.GetName().Name).Append(".Resources.").Append(".Enum.").Append(e.GetType().Name).Append(".").Append(System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
	////        ResourceManager _resources = new ResourceManager(resourceFile.ToString(), e.GetType().Assembly);

	////        //Load resource names for the assembly and check if the resource exists
	////        string[] arr = e.GetType().Assembly.GetManifestResourceNames();
	////        foreach (string s in arr) {
	////            if (s.Equals(resourceFile.ToString() + ".resources")) {
	////                string rk = String.Format("{0}.{1}", e.GetType(), e);

	////                if (_resources.GetString(rk) != null) {
	////                    localizedDescription = _resources.GetString(rk);
	////                }
	////                break;
	////            }
	////        }
	////        return localizedDescription;
	////    }
	////}
}
