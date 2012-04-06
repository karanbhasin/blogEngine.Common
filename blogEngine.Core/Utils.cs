using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Xml;

//using BlogEngine.Core.Web.Controls;
//using BlogEngine.Core.Web.Extensions;

namespace blogEngine.Core
{
    

    /// <summary>
    /// Utilities for the entire solution to use.
    /// </summary>
    public static class Utils
    {
        #region Constants and Fields

        /// <summary>
        /// The pattern.
        /// </summary>
        private const string Pattern = "<head.*<link( [^>]*title=\"{0}\"[^>]*)>.*</head>";

        /// <summary>
        /// The href regex.
        /// </summary>
        private static readonly Regex HrefRegex = new Regex(
            "href=\"(.*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// The regex between tags.
        /// </summary>
        private static readonly Regex RegexBetweenTags = new Regex(@">\s+", RegexOptions.Compiled);

        /// <summary>
        /// The regex line breaks.
        /// </summary>
        private static readonly Regex RegexLineBreaks = new Regex(@"\n\s+", RegexOptions.Compiled);

        /// <summary>
        /// The regex mobile.
        /// </summary>
        private static readonly Regex RegexMobile =
            new Regex(
                ConfigurationManager.AppSettings.Get("BlogEngine.MobileDevices"), 
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// The regex strip html.
        /// </summary>
        private static readonly Regex RegexStripHtml = new Regex("<[^>]*>", RegexOptions.Compiled);

        /// <summary>
        ///     The mono int.
        /// </summary>
        private static int mono;

        /// <summary>
        ///     The relative web root.
        /// </summary>
        private static string relativeWebRoot;

        #endregion

        #region Events

        /// <summary>
        ///     Occurs after an e-mail has been sent. The sender is the MailMessage object.
        /// </summary>
        public static event EventHandler<EventArgs> EmailFailed;

        /// <summary>
        ///     Occurs after an e-mail has been sent. The sender is the MailMessage object.
        /// </summary>
        public static event EventHandler<EventArgs> EmailSent;

        /// <summary>
        ///     Occurs when a message will be logged. The sender is a string containing the log message.
        /// </summary>
        public static event EventHandler<EventArgs> OnLog;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the absolute root of the website.
        /// </summary>
        /// <value>A string that ends with a '/'.</value>
        public static Uri AbsoluteWebRoot
        {
            get
            {
                // if (_AbsoluteWebRoot == null)
                // {
                var context = HttpContext.Current;
                if (context == null)
                {
                    throw new WebException("The current HttpContext is null");
                }

                if (context.Items["absoluteurl"] == null)
                {
                    context.Items["absoluteurl"] =
                        new Uri(context.Request.Url.GetLeftPart(UriPartial.Authority) + RelativeWebRoot);
                }

                return context.Items["absoluteurl"] as Uri;

                // _AbsoluteWebRoot = new Uri(context.Request.Url.GetLeftPart(UriPartial.Authority) + RelativeWebRoot);// new Uri(context.Request.Url.Scheme + "://" + context.Request.Url.Authority + RelativeWebRoot);
                // }
                // return _AbsoluteWebRoot;
            }
        }

        /// <summary>
        ///     Gets the relative URL of the blog feed. If a Feedburner username
        ///     is entered in the admin settings page, it will return the 
        ///     absolute Feedburner URL to the feed.
        /// </summary>
        public static string FeedUrl
        {
            get
            {
                return !string.IsNullOrEmpty(BlogSettings.Instance.AlternateFeedUrl)
                           ? BlogSettings.Instance.AlternateFeedUrl
                           : string.Format("{0}syndication.axd", AbsoluteWebRoot);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether we're running under Linux or a Unix variant.
        /// </summary>
        /// <value><c>true</c> if Linux/Unix; otherwise, <c>false</c>.</value>
        public static bool IsLinux
        {
            get
            {
                var p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 128);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the client is a mobile device.
        /// </summary>
        /// <value><c>true</c> if this instance is mobile; otherwise, <c>false</c>.</value>
        public static bool IsMobile
        {
            get
            {
                var context = HttpContext.Current;
                if (context != null)
                {
                    var request = context.Request;
                    if (request.Browser.IsMobileDevice)
                    {
                        return true;
                    }

                    if (!string.IsNullOrEmpty(request.UserAgent) && RegexMobile.IsMatch(request.UserAgent))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether we're running under Mono.
        /// </summary>
        /// <value><c>true</c> if Mono; otherwise, <c>false</c>.</value>
        public static bool IsMono
        {
            get
            {
                if (mono == 0)
                {
                    mono = Type.GetType("Mono.Runtime") != null ? 1 : 2;
                }

                return mono == 1;
            }
        }

        /// <summary>
        ///     Gets the relative root of the website.
        /// </summary>
        /// <value>A string that ends with a '/'.</value>
        public static string RelativeWebRoot
        {
            get
            {
                return relativeWebRoot ??
                       (relativeWebRoot =
                        VirtualPathUtility.ToAbsolute(ConfigurationManager.AppSettings["BlogEngine.VirtualPath"]));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method returns all code assemblies in app_code
        ///     If app_code has subdirectories for c#, vb.net etc
        ///     Each one will come back as a separate assembly
        ///     So we can support extensions in multiple languages
        /// </summary>
        /// <returns>
        /// List of code assemblies
        /// </returns>
        public static ArrayList CodeAssemblies()
        {
            var codeAssemblies = new ArrayList();
            CompilationSection s = null;
            try
            {
                var assemblyName = "__code";
                try
                {
                    s = (CompilationSection)WebConfigurationManager.GetSection("system.web/compilation");
                }
                catch (SecurityException)
                {
                    // No read permissions on web.config due to the trust level (must be High or Full)
                }

                if (s != null && s.CodeSubDirectories != null && s.CodeSubDirectories.Count > 0)
                {
                    for (var i = 0; i < s.CodeSubDirectories.Count; i++)
                    {
                        assemblyName = string.Format("App_SubCode_{0}", s.CodeSubDirectories[i].DirectoryName);
                        codeAssemblies.Add(Assembly.Load(assemblyName));
                    }
                }
                else
                {
                    var t = Type.GetType("Mono.Runtime");
                    if (t != null)
                    {
                        assemblyName = "App_Code";
                    }

                    codeAssemblies.Add(Assembly.Load(assemblyName));
                }

                GetCompiledExtensions(codeAssemblies);
            }
            catch (FileNotFoundException)
            {
                /*ignore - code directory has no files*/
            }

            return codeAssemblies;
        }

        /// <summary>
        /// Converts a relative URL to an absolute one.
        /// </summary>
        /// <param name="relativeUri">
        /// The relative Uri.
        /// </param>
        /// <returns>
        /// The absolute Uri.
        /// </returns>
        public static Uri ConvertToAbsolute(Uri relativeUri)
        {
            return ConvertToAbsolute(relativeUri.ToString());
        }

        /// <summary>
        /// Converts a relative URL to an absolute one.
        /// </summary>
        /// <param name="relativeUri">
        /// The relative Uri.
        /// </param>
        /// <returns>
        /// The absolute Uri.
        /// </returns>
        public static Uri ConvertToAbsolute(string relativeUri)
        {
            if (String.IsNullOrEmpty(relativeUri))
            {
                throw new ArgumentNullException("relativeUri");
            }

            var absolute = AbsoluteWebRoot.ToString();
            var index = absolute.LastIndexOf(RelativeWebRoot);

            return new Uri(absolute.Substring(0, index) + relativeUri);
        }

        /// <summary>
        /// Downloads a web page from the Internet and returns the HTML as a string. .
        /// </summary>
        /// <param name="url">
        /// The URL to download from.
        /// </param>
        /// <returns>
        /// The HTML or null if the URL isn't valid.
        /// </returns>
        public static string DownloadWebPage(Uri url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers["Accept-Encoding"] = "gzip";
                request.Headers["Accept-Language"] = "en-us";
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (var response = request.GetResponse())
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// Extract file name from given phisical server path
        /// </summary>
        /// <param name="path">
        /// The Server path.
        /// </param>
        /// <returns>
        /// The File Name.
        /// </returns>
        public static string ExtractFileNameFromPath(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            return path.Substring(path.LastIndexOf("\\")).Replace("\\", string.Empty);
        }

        /// <summary>
        /// Finds semantic links in a given HTML document.
        /// </summary>
        /// <param name="type">
        /// The type of link. Could be foaf, apml or sioc.
        /// </param>
        /// <param name="html">
        /// The HTML to look through.
        /// </param>
        /// <returns>
        /// A list of Uri.
        /// </returns>
        public static List<Uri> FindLinks(string type, string html)
        {
            var matches = Regex.Matches(
                html, string.Format(Pattern, type), RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var urls = new List<Uri>();

            foreach (var hrefMatch in
                matches.Cast<Match>().Where(match => match.Groups.Count == 2).Select(match => match.Groups[1].Value).
                    Select(link => HrefRegex.Match(link)).Where(hrefMatch => hrefMatch.Groups.Count == 2))
            {
                Uri url;
                var value = hrefMatch.Groups[1].Value;
                if (Uri.TryCreate(value, UriKind.Absolute, out url))
                {
                    urls.Add(url);
                }
            }

            return urls;
        }

        /// <summary>
        /// Finds the semantic documents from a URL based on the type.
        /// </summary>
        /// <param name="url">
        /// The URL of the semantic document or a document containing semantic links.
        /// </param>
        /// <param name="type">
        /// The type. Could be "foaf", "apml" or "sioc".
        /// </param>
        /// <returns>
        /// A dictionary of the semantic documents. The dictionary is empty if no documents were found.
        /// </returns>
        public static Dictionary<Uri, XmlDocument> FindSemanticDocuments(Uri url, string type)
        {
            var list = new Dictionary<Uri, XmlDocument>();

            var content = DownloadWebPage(url);
            if (!string.IsNullOrEmpty(content))
            {
                var upper = content.ToUpperInvariant();

                if (upper.Contains("</HEAD") && upper.Contains("</HTML"))
                {
                    var urls = FindLinks(type, content);
                    foreach (var xmlUrl in urls)
                    {
                        var doc = LoadDocument(url, xmlUrl);
                        if (doc != null)
                        {
                            list.Add(xmlUrl, doc);
                        }
                    }
                }
                else
                {
                    var doc = LoadDocument(url, url);
                    if (doc != null)
                    {
                        list.Add(url, doc);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Returns the default culture.  This is either the culture specified in the blog settings,
        ///     or the default culture installed with the operating system.
        /// </summary>
        /// <returns>
        /// The default culture.
        /// </returns>
        public static CultureInfo GetDefaultCulture()
        {
            if (string.IsNullOrEmpty(BlogSettings.Instance.Culture) ||
                BlogSettings.Instance.Culture.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.InstalledUICulture;
            }

            return CultureInfo.CreateSpecificCulture(BlogSettings.Instance.Culture);
        }

        /// <summary>
        /// Gets the sub domain.
        /// </summary>
        /// <param name="url">
        /// The URL to get the sub domain from.
        /// </param>
        /// <returns>
        /// The sub domain.
        /// </returns>
        public static string GetSubDomain(Uri url)
        {
            if (url.HostNameType == UriHostNameType.Dns)
            {
                var host = url.Host;
                if (host.Split('.').Length > 2)
                {
                    var lastIndex = host.LastIndexOf(".");
                    var index = host.LastIndexOf(".", lastIndex - 1);
                    return host.Substring(0, index);
                }
            }

            return null;
        }

        /// <summary>
        /// Encrypts a string using the SHA256 algorithm.
        /// </summary>
        /// <param name="plainMessage">
        /// The plain Message.
        /// </param>
        /// <returns>
        /// The hash password.
        /// </returns>
        public static string HashPassword(string plainMessage)
        {
            var data = Encoding.UTF8.GetBytes(plainMessage);
            using (HashAlgorithm sha = new SHA256Managed())
            {
                sha.TransformFinalBlock(data, 0, data.Length);
                return Convert.ToBase64String(sha.Hash);
            }
        }

        /// <summary>
        /// Run through all code assemblies and creates object
        ///     instance for types marked with extension attribute
        /// </summary>
        //public static void LoadExtensions()
        //{
        //    var codeAssemblies = CodeAssemblies();
        //    var sortedExtensions = new List<SortedExtension>();

        //    foreach (Assembly a in codeAssemblies)
        //    {
        //        var types = a.GetTypes();
        //        sortedExtensions.AddRange(
        //            from type in types
        //            let attributes = type.GetCustomAttributes(typeof(ExtensionAttribute), false)
        //            from attribute in attributes
        //            where attribute.GetType().Name == "ExtensionAttribute"
        //            let ext = (ExtensionAttribute)attribute
        //            select new SortedExtension(ext.Priority, type.Name, type.FullName));

        //        sortedExtensions.Sort(
        //            (e1, e2) =>
        //            e1.Priority == e2.Priority
        //                ? string.CompareOrdinal(e1.Name, e2.Name)
        //                : e1.Priority.CompareTo(e2.Priority));

        //        foreach (var x in sortedExtensions.Where(x => ExtensionManager.ExtensionEnabled(x.Name)))
        //        {
        //            a.CreateInstance(x.Type);
        //        }
        //    }

        //    // initialize comment rules and filters
        //    CommentHandlers.Listen();
        //}

        /// <summary>
        /// Sends a message to any subscribed log listeners.
        /// </summary>
        /// <param name="message">
        /// The message to be logged.
        /// </param>
        public static void Log(object message)
        {
            if (OnLog != null)
            {
                OnLog(message, new EventArgs());
            }
        }

        /// <summary>
        /// Generates random password for password reset
        /// </summary>
        /// <returns>
        /// Random password
        /// </returns>
        public static string RandomPassword()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            var password = string.Empty;
            var random = new Random();

            for (var i = 0; i < 8; i++)
            {
                var x = random.Next(1, chars.Length);
                if (!password.Contains(chars.GetValue(x).ToString()))
                {
                    password += chars.GetValue(x);
                }
                else
                {
                    i--;
                }
            }

            return password;
        }

        /// <summary>
        /// Removes the HTML whitespace.
        /// </summary>
        /// <param name="html">
        /// The HTML to remove the whitespace from.
        /// </param>
        /// <returns>
        /// The html with the whitespace removed.
        /// </returns>
        public static string RemoveHtmlWhitespace(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            html = RegexBetweenTags.Replace(html, "> ");
            html = RegexLineBreaks.Replace(html, string.Empty);

            return html.Trim();
        }

        /// <summary>
        /// Strips all illegal characters from the specified title.
        /// </summary>
        /// <param name="text">
        /// The text to strip.
        /// </param>
        /// <returns>
        /// The remove illegal characters.
        /// </returns>
        public static string RemoveIllegalCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            text = text.Replace(":", string.Empty);
            text = text.Replace("/", string.Empty);
            text = text.Replace("?", string.Empty);
            text = text.Replace("#", string.Empty);
            text = text.Replace("[", string.Empty);
            text = text.Replace("]", string.Empty);
            text = text.Replace("@", string.Empty);
            text = text.Replace("*", string.Empty);
            text = text.Replace(".", string.Empty);
            text = text.Replace(",", string.Empty);
            text = text.Replace("\"", string.Empty);
            text = text.Replace("&", string.Empty);
            text = text.Replace("'", string.Empty);
            text = text.Replace(" ", "-");
            text = RemoveDiacritics(text);
            text = RemoveExtraHyphen(text);

            return HttpUtility.UrlEncode(text).Replace("%", string.Empty);
        }

        /// <summary>
        /// Sends a MailMessage object using the SMTP settings.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void SendMailMessage(MailMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            try
            {
                message.IsBodyHtml = true;
                message.BodyEncoding = Encoding.UTF8;
                var smtp = new SmtpClient(BlogSettings.Instance.SmtpServer);

                // don't send credentials if a server doesn't require it,
                // linux smtp servers don't like that 
                if (!string.IsNullOrEmpty(BlogSettings.Instance.SmtpUserName))
                {
                    smtp.Credentials = new NetworkCredential(
                        BlogSettings.Instance.SmtpUserName, BlogSettings.Instance.SmtpPassword);
                }

                smtp.Port = BlogSettings.Instance.SmtpServerPort;
                smtp.EnableSsl = BlogSettings.Instance.EnableSsl;
                smtp.Send(message);
                OnEmailSent(message);
            }
            catch (SmtpException)
            {
                OnEmailFailed(message);
            }
            finally
            {
                // Remove the pointer to the message object so the GC can close the thread.
                message.Dispose();
            }
        }

        /// <summary>
        /// Sends the mail message asynchronously in another thread.
        /// </summary>
        /// <param name="message">
        /// The message to send.
        /// </param>
        public static void SendMailMessageAsync(MailMessage message)
        {
            ThreadPool.QueueUserWorkItem(delegate { SendMailMessage(message); });
        }

        /// <summary>
        /// Writes ETag and Last-Modified headers and sets the conditional get headers.
        /// </summary>
        /// <param name="date">
        /// The date for the headers.
        /// </param>
        /// <returns>
        /// The set conditional get headers.
        /// </returns>
        public static bool SetConditionalGetHeaders(DateTime date)
        {
            // SetLastModified() below will throw an error if the 'date' is a future date.
            // If the date is 1/1/0001, Mono will throw a 404 error
            if (date > DateTime.Now || date.Year < 1900)
            {
                date = DateTime.Now;
            }

            var response = HttpContext.Current.Response;
            var request = HttpContext.Current.Request;

            var etag = string.Format("\"{0}\"", date.Ticks);
            var incomingEtag = request.Headers["If-None-Match"];

            DateTime incomingLastModifiedDate;
            DateTime.TryParse(request.Headers["If-Modified-Since"], out incomingLastModifiedDate);

            response.Cache.SetLastModified(date);
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.Cache.SetETag(etag);

            if (String.Compare(incomingEtag, etag) == 0 || incomingLastModifiedDate == date)
            {
                response.Clear();
                response.StatusCode = (int)HttpStatusCode.NotModified;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Strips all HTML tags from the specified string.
        /// </summary>
        /// <param name="html">
        /// The string containing HTML
        /// </param>
        /// <returns>
        /// A string without HTML tags
        /// </returns>
        public static string StripHtml(string html)
        {
            return string.IsNullOrEmpty(html) ? string.Empty : RegexStripHtml.Replace(html, string.Empty);
        }

        /// <summary>
        /// Translates the specified string using the resource files.
        /// </summary>
        /// <param name="text">
        /// The text to translate.
        /// </param>
        /// <returns>
        /// The translate.
        /// </returns>
        public static string Translate(string text)
        {
            return Translate(text, null, null);
        }

        /// <summary>
        /// Translates the specified string using the resource files.  If a translation
        ///     is not found, defaultValue will be returned.
        /// </summary>
        /// <param name="text">
        /// The text to translate.
        /// </param>
        /// <param name="defaultValue">
        /// The default Value.
        /// </param>
        /// <returns>
        /// The translate.
        /// </returns>
        public static string Translate(string text, string defaultValue)
        {
            return Translate(text, defaultValue, null);
        }

        /// <summary>
        /// Translates the specified string using the resource files and specified culture.
        ///     If a translation is not found, defaultValue will be returned.
        /// </summary>
        /// <param name="text">
        /// The text to translate.
        /// </param>
        /// <param name="defaultValue">
        /// The default Value.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The translate.
        /// </returns>
        public static string Translate(string text, string defaultValue, CultureInfo culture)
        {
            var resource = culture == null
                               ? HttpContext.GetGlobalResourceObject("labels", text)
                               : HttpContext.GetGlobalResourceObject("labels", text, culture);

            return resource != null
                       ? resource.ToString()
                       : (string.IsNullOrEmpty(defaultValue)
                              ? string.Format("Missing Resource [{0}]", text)
                              : defaultValue);
        }

        #endregion

        #region Methods

        /// <summary>
        /// To support compiled extensions
        ///     This methed looks for DLLs in the "/bin" folder
        ///     and if assembly compiled with configuration
        ///     attributed set to "BlogEngineExtension" it will
        ///     be added to the list of code assemlies
        /// </summary>
        /// <param name="assemblies">
        /// List of code assemblies
        /// </param>
        private static void GetCompiledExtensions(ArrayList assemblies)
        {
            var s = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "bin");
            var fileEntries = Directory.GetFiles(s);
            foreach (var asm in from fileName in fileEntries
                                where fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                                select Assembly.LoadFrom(fileName)
                                into asm
                                let attr = asm.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false)
                                where attr.Length > 0
                                let aca = (AssemblyConfigurationAttribute)attr[0]
                                where aca != null && aca.Configuration == "BlogEngineExtension"
                                select asm)
            {
                assemblies.Add(asm);
            }
        }

        /// <summary>
        /// Loads the document.
        /// </summary>
        /// <param name="url">
        /// The URL of the document to load.
        /// </param>
        /// <param name="xmlUrl">
        /// The XML URL.
        /// </param>
        /// <returns>
        /// The XmlDocument.
        /// </returns>
        private static XmlDocument LoadDocument(Uri url, Uri xmlUrl)
        {
            string absoluteUrl;

            if (url.IsAbsoluteUri)
            {
                absoluteUrl = url.ToString();
            }
            else if (!url.ToString().StartsWith("/"))
            {
                absoluteUrl = url + xmlUrl.ToString();
            }
            else
            {
                absoluteUrl = string.Format("{0}://{1}{2}", url.Scheme, url.Authority, xmlUrl);
            }

            var readerSettings = new XmlReaderSettings
                {
                   ProhibitDtd = false, MaxCharactersFromEntities = 1024, XmlResolver = new XmlSafeResolver() 
                };

            XmlDocument doc;
            using (var reader = XmlReader.Create(absoluteUrl, readerSettings))
            {
                doc = new XmlDocument();

                try
                {
                    doc.Load(reader);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return doc;
        }

        /// <summary>
        /// The on email failed.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private static void OnEmailFailed(MailMessage message)
        {
            if (EmailFailed != null)
            {
                EmailFailed(message, new EventArgs());
            }
        }

        /// <summary>
        /// The on email sent.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private static void OnEmailSent(MailMessage message)
        {
            if (EmailSent != null)
            {
                EmailSent(message, new EventArgs());
            }
        }

        /// <summary>
        /// Removes the diacritics.
        /// </summary>
        /// <param name="text">
        /// The text to remove diacritics from.
        /// </param>
        /// <returns>
        /// The string with the diacritics removed.
        /// </returns>
        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in
                normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
            {
                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Removes the extra hyphen.
        /// </summary>
        /// <param name="text">
        /// The text to remove the extra hyphen from.
        /// </param>
        /// <returns>
        /// The text with the extra hyphen removed.
        /// </returns>
        private static string RemoveExtraHyphen(string text)
        {
            if (text.Contains("--"))
            {
                text = text.Replace("--", "-");
                return RemoveExtraHyphen(text);
            }

            return text;
        }

        #endregion
    }
}