using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Foundation.Common.Entity;
using System.Web.Routing;

namespace System.Web.Mvc {
    /// <summary>
    /// Purpose of the class is to extend the MVC Html helper methods
    /// </summary>
    public static class HtmlHelperExtensions {

        #region Public Methods
        private static string _assetUrl;
        public static string AssetUrl {
            get {
                if (_assetUrl != null)
                    return _assetUrl;
                _assetUrl = String.Format(Foundation.Common.Util.ConfigUtil.GetPrefixedAppSetting("Web.AssetsUrl"), GetHttpProtocol());

                return _assetUrl;
            }
        }

        /// <summary>
        /// Creates a reference to a css style sheet
        /// </summary>
        /// <param name="source"></param>
        /// <param name="path">file name to the style sheet</param>
        /// <returns></returns>
        public static string StylesheetLinkTag(this System.Web.Mvc.HtmlHelper source, string path) {
            string ver = GetVersionForQueryString();
            string link = String.Format("\r\n<link href=\"{0}stylesheets/{1}{2}\" rel=\"stylesheet\" type=\"text/css\" />\r\n", AssetUrl, path, ver);

            return link;
        }

        /// <summary>
        /// Creates a reference to a javascript file
        /// </summary>
        /// <param name="source"></param>
        /// <param name="path">file name to the javascript file</param>
        /// <returns></returns>
        public static string JavascriptIncludeTag(this System.Web.Mvc.HtmlHelper source, string path) {
            string ver = GetVersionForQueryString();
            string js = String.Format("\r\n<script src=\"{0}javascripts/{1}{2}\" type=\"text/javascript\"></script>\r\n", AssetUrl, path, ver);

            return js;
        }

        #region Submit Button
        public static string SubmitButton(this System.Web.Mvc.HtmlHelper source, string text, bool includeWait) {
            return SubmitButton(source, text, null, includeWait);
        }

        public static string SubmitButton(this System.Web.Mvc.HtmlHelper source, string text, string id, bool includeWait) {
            return SubmitButton(source, text, id, includeWait, null);
        }

        public static string SubmitButton(this System.Web.Mvc.HtmlHelper source, string text, string id, bool includeWait, string attributes) {
            string html = string.Empty;
            string onClickString = string.Empty;

            if (String.IsNullOrEmpty(id)) {
                id = "submitQuery";
            }

            if (!string.IsNullOrEmpty(attributes)) {
                int onClickStart = attributes.IndexOf("onclick");

                if (onClickStart > -1) {
                    // Get first quote
                    int firstQuoteIndex = attributes.IndexOf("\"", onClickStart);
                    if (firstQuoteIndex > -1) {
                        // Get end quote
                        int secondQuoteIndex = attributes.IndexOf("\"", firstQuoteIndex + 1);
                        onClickString = attributes.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);

                        // Remove the onclick section from attributes
                        attributes = attributes.Substring(0, onClickStart) + attributes.Substring(secondQuoteIndex + 1);
                    }
                }
            }

            if (includeWait) {
                html = "";

                if (id == "submitQuery") {
                    html += "<span class=\"submit_wait\"><input type=\"submit\" id=\"" + id + "\" value=\"" + text + "\" onclick=\"showWaitBeforeSubmit(this.id);{1}\" {0} /></span>";
                }
                else {
                    html += "<span class=\"submit_wait\"><input type=\"submit\" id=\"" + id + "\" name=\"" + id + "\" value=\"" + text + "\" onclick=\"showWaitBeforeSubmit(this.id);{1}\" {0} /></span>";
                }

                html = String.Format(html, attributes ?? "", onClickString ?? "");
            }
            else {
                if (id == "submitQuery") {
                    html += "<input type=\"submit\" id=\"" + id + "\" value=\"" + text + "\" />";
                }
                else {
                    html += "<input type=\"submit\" id=\"" + id + "\" name=\"" + id + "\" value=\"" + text + "\" />";
                }
            }

            return html;
        }
        #endregion Submit Button

        #region Date Controls
        //public static string DateControls(this System.Web.Mvc.HtmlHelper source, string id, string labelText) {
        //    return DateControls(source, id, labelText, Foundation.Common.Util.NullUtil.DateTimeNull);
        //}

        //public static string DateControls(this System.Web.Mvc.HtmlHelper source, string id, string labelText, DateTime date) {
        //    string dt = string.Empty;
        //    if (!Foundation.Common.Util.NullUtil.IsNull(date)) {
        //        dt = date.ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
        //        //IFormatProvider culture = new System.Globalization.CultureInfo(System.Web.HttpContext.Current.Request.Cookies["culture"].Value, true);
        //        //dt = date.ToString(culture);
        //    }

        //    return DateControls(source, id, labelText, dt, "");
        //}

        //public static string DateControls(this System.Web.Mvc.HtmlHelper source, string id, string labelText, string dateString, string attrs) {

        //    return DateControls(source, id, id, labelText, dateString, attrs);
        //}

        public static string DateControls(this System.Web.Mvc.HtmlHelper source, string id, string name, string labelText, string dateString, string attrs, string errorMessage) {

            string formatClassName = "format-m-d-y";
            string formatDisplay = "MM / DD / YYYY";

            if (System.Globalization.CultureInfo.CurrentCulture.Name != "en-US") {
                formatClassName = "format-d-m-y";
                formatDisplay = "DD / MM / YYYY";
            }


            string html = "<div class=\"date_control\">";
            if (!String.IsNullOrEmpty(labelText)) {
                html += "<label for=\"{0}\">{2}</label>";
            }

            DateTime comp;
            if (DateTime.TryParse(dateString, out comp) && comp.Date == DateTime.MinValue.Date) {
                dateString = string.Empty;
            }

            html += "<input type=\"text\" id=\"{0}\" name=\"{1}\" value=\"" + dateString + "\" maxlength=\"10\" class=\"allow_date_only {3} no-transparency\" {4} />&nbsp;<span id=\"{0}_error_message\" style=\"display: none;\" class=\"attribute_invalid_date\">{6}</span><br /><span class=\"date_display\">{5}</span>";
            html += "</div>";

            return String.Format(html, id, name, labelText, formatClassName, attrs, formatDisplay, errorMessage);
        }

        public static string DateControls(this System.Web.Mvc.HtmlHelper source, string id, string name, string labelText, string dateString, string attrs) {

            string formatClassName = "format-m-d-y";
            string formatDisplay = "MM / DD / YYYY";

            if (System.Globalization.CultureInfo.CurrentCulture.Name != "en-US") {
                formatClassName = "format-d-m-y";
                formatDisplay = "DD / MM / YYYY";
            }


            string html = "<div class=\"date_control\">";
            if (!String.IsNullOrEmpty(labelText)) {
                html += "<label for=\"{0}\">{2}</label>";
            }

            DateTime comp;
            if (DateTime.TryParse(dateString, out comp) && comp.Date == DateTime.MinValue.Date) {
                dateString = string.Empty;
            }
            //if (DateTime.Parse(dateString, new System.Globalization.CultureInfo(System.Web.HttpContext.Current.Request.Cookies["culture"].Value, true)).Date == DateTime.MinValue.Date)
            //    dateString = string.Empty;

            html += "<input type=\"text\" id=\"{0}\" name=\"{1}\" value=\"" + dateString + "\" maxlength=\"10\" class=\"allow_date_only {3} no-transparency\" {4} /><br /><span class=\"date_display\">{5}</span>";
            html += "</div>";

            return String.Format(html, id, name, labelText, formatClassName, attrs, formatDisplay);
        }
        #endregion Date Controls

        #region Image Tag
        public static string ImageTag(this System.Web.Mvc.HtmlHelper source, string src, string alt, string width, string height) {
            return ImageTag(source, src, alt, width, height, null);
        }

        public static string ImageTag(this System.Web.Mvc.HtmlHelper source, string src, string alt, string width, string height, string attributes) {
            return ImageTag(source, string.Empty, src, alt, width, height, attributes);
        }

        public static string ImageTag(this System.Web.Mvc.HtmlHelper source, string id, string src, string alt, string width, string height, string attributes) {
            if (!String.IsNullOrEmpty(width)) {
                attributes += " width=\"" + width + "\"";
            }

            if (!String.IsNullOrEmpty(height)) {
                attributes += " height=\"" + height + "\"";
            }

            return string.IsNullOrEmpty(id) ? String.Format("<img src=\"{0}\" alt=\"{1}\" {2} />", ImageAssetsUrl(src), alt, attributes ?? "") : String.Format("<img src=\"{0}\" alt=\"{1}\" {2} id=\"{3}\"/>", ImageAssetsUrl(src), alt, attributes ?? "", id);
        }
        #endregion Image Tag

        #region Html Action Link
        public static string HtmlActionLink(this System.Web.Mvc.HtmlHelper source, string html, string actionName, string controllerName) {
            return HtmlActionLink(source, html, actionName, controllerName, null);
        }

        public static string HtmlActionLink(this System.Web.Mvc.HtmlHelper source, string html, string actionName, string controllerName, object routeValues) {
            return HtmlActionLink(source, html, actionName, controllerName, routeValues, null);
        }

        public static string HtmlActionLink(this System.Web.Mvc.HtmlHelper source, string html, string actionName, string controllerName, object routeValues, object htmlAttributes) {

            UrlHelper urlHelper = new UrlHelper(source.ViewContext.RequestContext);
            string url = urlHelper.Action(actionName, controllerName, routeValues);

            TagBuilder tagWrapper = new TagBuilder("span");

            if (htmlAttributes != null) {
                tagWrapper.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);
            }

            // Create an a tag from the individuals name
            TagBuilder tagBuilder = new TagBuilder("a") { InnerHtml = html };
            tagBuilder.MergeAttribute("href", url);

            tagWrapper.InnerHtml = tagBuilder.ToString(TagRenderMode.Normal);

            // If there are html attributes, render the tag wrapper, if not, there is no need for the wrapper, just render the builder
            if (htmlAttributes != null) {
                return tagWrapper.ToString(TagRenderMode.Normal);
            }
            else {
                return tagBuilder.ToString(TagRenderMode.Normal);
            }
        }

        #endregion Html Action Link

        public static string ImageAssetsUrl(string src) {
            String ver = GetVersionForQueryString();
            return String.Format("{0}images/{1}{2}", AssetUrl, src, ver);
        }

        //public static string ErrorMessagesFor(this System.Web.Mvc.HtmlHelper source, EntityFoundation entity) {
        //    StringBuilder sb = new StringBuilder();

        //    if (entity != null && entity.ErrorMessages.Count > 0) {
        //        sb.Append("<div class=\"error_msgs_for\">");
        //        sb.Append("<h2>The following error");
        //        if (entity.ErrorMessages.Count > 1) {
        //            sb.Append("s");
        //        }
        //        sb.Append(" occurred&hellip;</h2>");
        //        sb.Append("<ul>");
        //        foreach (string key in entity.ErrorMessages.Keys) {
        //            sb.Append("<li>").Append(entity.ErrorMessages[key]).Append("</li>");
        //        }
        //        sb.Append("</ul>");
        //        sb.Append("</div>");
        //    }

        //    return sb.ToString();
        //}
        #endregion Public Methods

        #region Private Methods
        private static string GetHttpProtocol() {
            return System.Web.HttpContext.Current.Request.IsSecureConnection ? "https://" : "http://";
        }

        private static string GetVersionForQueryString() {
            string ver = Foundation.Common.Util.ConfigUtil.GetAppSetting("CurrentVersion");
            ver = String.IsNullOrEmpty(ver) ? "" : "?" + ver.Replace(".", "");

            return ver;
        }

        private static string ApplicationName {
            get {
                return Foundation.Common.Util.ConfigUtil.GetAppSetting("Application.Name");
            }
        }
        #endregion Private Methods

    }
}
