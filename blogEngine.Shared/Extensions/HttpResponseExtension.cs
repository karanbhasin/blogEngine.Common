using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Web {
	public static class HttpResponseExtension {

		public static void SafeRedirect(this HttpResponse response, string url) {
			response.Redirect(url, false);
			if (HttpContext.Current != null && HttpContext.Current.ApplicationInstance != null) {
				HttpContext.Current.ApplicationInstance.CompleteRequest();
			}

		}

	}
}