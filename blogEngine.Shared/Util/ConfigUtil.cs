using System;
using System.Configuration;
using System.Web.Configuration;

namespace Foundation.Common.Util {
    /// <summary>
    /// Summary description for ConfigUtil.
    /// </summary>
    public class ConfigUtil {
        private static string ConvertAppKey(string setting) {

            string keyPrefix = ConfigurationManager.AppSettings["Application.Name"];
            string rVal = setting;

            if (keyPrefix != string.Empty) {
                rVal = keyPrefix + "." + setting;
            }

            return rVal;
        }

        public static string GetAppSetting(string sSetting) {
            if (ConfigurationManager.AppSettings[sSetting] != null) {
                return ConfigurationManager.AppSettings[sSetting];
            }
            else {
                throw new NullReferenceException(string.Format("The app setting {0} was not found on machine {1}", sSetting, Environment.MachineName));
            }
        }

        public static bool HasAppSetting(string sSetting) {
            return ConfigurationManager.AppSettings[sSetting] != null;
        }

        public static string GetPrefixedAppSetting(string setting) {
            string appKey = ConvertAppKey(setting);
            if (ConfigurationManager.AppSettings[appKey] != null) {
                return ConfigurationManager.AppSettings[appKey];
            }
            else {
                throw new NullReferenceException(string.Format("The app setting {0} was not found on machine {1}", appKey, Environment.MachineName));
            }
        }

        public static string GetVersionForQueryString() {
            string ver = Foundation.Common.Util.ConfigUtil.GetAppSetting("CurrentVersion");
            ver = String.IsNullOrEmpty(ver) ? "" : "?" + ver.Replace(".", "");

            return ver;
        }

        public static System.Web.SessionState.SessionStateMode GetSessionStateMode() {
            SessionStateSection sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
            return sessionStateSection.Mode;
        }
    }
}