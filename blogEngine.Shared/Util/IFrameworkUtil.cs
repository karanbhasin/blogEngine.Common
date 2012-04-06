using System;
using System.Collections.Specialized;

using Foundation.Common.ViewProxies;

namespace Foundation.Common.Util {
	public interface IFrameworkUtil {
		string Testing();
		ViewProxy CreateViewFromParameterCollection(string viewProxyName, NameValueCollection parms);
		ViewProxy CreateEmptyView(string viewProxyName);
	}
}
