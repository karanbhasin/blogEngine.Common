namespace BlogEngine.Core.Web.HttpHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;

    using BlogEngine.Core.Web.HttpModules;

    /// <summary>
    /// Removes whitespace in all stylesheets added to the 
    ///     header of the HTML document in site.master.
    /// </summary>
    public class JavaScriptHandler : IHttpHandler
    {
        #region Properties

        /// <summary>
        ///     Gets a value indicating whether another request can use the <see cref = "T:System.Web.IHttpHandler"></see> instance.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref = "T:System.Web.IHttpHandler"></see> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Implemented Interfaces

        #region IHttpHandler

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom 
        ///     HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
        /// </summary>
        /// <param name="context">
        /// An <see cref="T:System.Web.HttpContext"></see> object that provides 
        ///     references to the intrinsic server objects 
        ///     (for example, Request, Response, Session, and Server) used to service HTTP requests.
        /// </param>
        public void ProcessRequest(HttpContext context)
        {
            var path = context.Request.QueryString["path"];

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var script = context.Cache[path] == null
                                ? (path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                       ? RetrieveRemoteScript(path)
                                       : RetrieveLocalScript(path))
                                : (string)context.Cache[path];

            if (string.IsNullOrEmpty(script))
            {
                return;
            }

            SetHeaders(script.GetHashCode(), context);
            context.Response.Write(script);

            if (BlogSettings.Instance.EnableHttpCompression)
            {
                CompressionModule.CompressResponse(context); // Compress(context);
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Checks whether to hard minify output.
        /// </summary>
        /// <param name="file">The file name.</param>
        /// <returns>Whether to hard minify output.</returns>
        private static bool HardMinify(string file)
        {
            var lookfor = ConfigurationManager.AppSettings.Get("BlogEngine.HardMinify").Split(
                new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            return lookfor.Any(file.Contains);
        }

        /// <summary>
        /// Retrieves the local script from the disk
        /// </summary>
        /// <param name="file">
        /// The file name.
        /// </param>
        /// <returns>
        /// The retrieve local script.
        /// </returns>
        private static string RetrieveLocalScript(string file)
        {
            if (!file.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityException("No access");
            }

            var path = HttpContext.Current.Server.MapPath(file);
            string script = null;

            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    script = reader.ReadToEnd();
                    script = StripWhitespace(script, HardMinify(file));
                    HttpContext.Current.Cache.Insert(file, script, new CacheDependency(path));
                }
            }

            return script;
        }

        /// <summary>
        /// Retrieves the specified remote script using a WebClient.
        /// </summary>
        /// <param name="file">
        /// The remote URL
        /// </param>
        /// <returns>
        /// The retrieve remote script.
        /// </returns>
        private static string RetrieveRemoteScript(string file)
        {
            string script = null;

            try
            {
                var url = new Uri(file, UriKind.Absolute);

                using (var client = new WebClient())
                {
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                    script = client.DownloadString(url);
                    script = StripWhitespace(script, HardMinify(file));
                    HttpContext.Current.Cache.Insert(
                        file, script, null, Cache.NoAbsoluteExpiration, new TimeSpan(3, 0, 0, 0));
                }
            }
            catch (SocketException)
            {
                // The remote site is currently down. Try again next time.
            }
            catch (UriFormatException)
            {
                // Only valid absolute URLs are accepted
            }

            return script;
        }

        /// <summary>
        /// This will make the browser and server keep the output
        ///     in its cache and thereby improve performance.
        /// </summary>
        /// <param name="hash">
        /// The hash number.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        private static void SetHeaders(int hash, HttpContext context)
        {
            context.Response.ContentType = "text/javascript";
            context.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;

            context.Response.Cache.SetExpires(DateTime.Now.ToUniversalTime().AddDays(7));
            context.Response.Cache.SetMaxAge(new TimeSpan(7, 0, 0, 0));
            context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

            var etag = string.Format("\"{0}\"", hash);
            var incomingEtag = context.Request.Headers["If-None-Match"];

            context.Response.Cache.SetETag(etag);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);

            if (String.Compare(incomingEtag, etag) != 0)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.NotModified;
            context.Response.SuppressContent = true;
        }

        /// <summary>
        /// Strips the whitespace from any .css file.
        /// </summary>
        /// <param name="body">
        /// The body string.
        /// </param>
        /// <param name="blogEngineScript">
        /// The is Blog Engine Script.
        /// </param>
        /// <returns>
        /// The strip whitespace.
        /// </returns>
        private static string StripWhitespace(string body, bool blogEngineScript)
        {
            var lines = body.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var emptyLines = new StringBuilder();
            foreach (var s in lines.Select(line => line.Trim()).Where(s => s.Length > 0 && !s.StartsWith("//")))
            {
                emptyLines.AppendLine(s.Trim());
            }

            body = emptyLines.ToString();

            if (blogEngineScript)
            {
                // mark strings and regular expressions
                var re =
                    new Regex(
                        "\"(([^\"\\r\\n])|(\\\"))*\"|'[^'\\r\\n]*'|/[^/\\*](?<![/\\S]/.)([^/\\\\\\r\\n]|\\\\.)*/(?=[ig]{0,2}[^\\S])",
                        RegexOptions.Compiled | RegexOptions.Multiline);
                var strs = new List<string>();
                var m = re.Matches(body);
                for (var i = 0; i < m.Count; i++)
                {
                    strs.Add(m[i].Value);

                    // replace string and regular expression with marker
                    var sb = new StringBuilder();
                    sb.Append("_____STRINGREGEX_");
                    sb.Append(i.ToString());
                    sb.Append("_STRINGREGEX_____");
                    body = re.Replace(body, sb.ToString(), 1);
                }

                // remove line comments
                body = Regex.Replace(body, "//.*[\r\n]", String.Empty, RegexOptions.Compiled | RegexOptions.ECMAScript);

                // remove C styles comments
                body = Regex.Replace(body, "/\\*.*?\\*/", String.Empty, RegexOptions.Compiled | RegexOptions.Singleline);

                // trim left
                body = Regex.Replace(body, "^\\s*", String.Empty, RegexOptions.Compiled | RegexOptions.Multiline);

                // trim right
                body = Regex.Replace(body, "\\s*[\\r\\n]", "\r\n", RegexOptions.Compiled | RegexOptions.ECMAScript);

                // remove whitespace beside of left curly braced
                body = Regex.Replace(body, "\\s*{\\s*", "{", RegexOptions.Compiled | RegexOptions.ECMAScript);

                // remove whitespace beside of right curly braced
                body = Regex.Replace(body, "\\s*}\\s*", "}", RegexOptions.Compiled | RegexOptions.ECMAScript);

                // remove whitespace beside of coma
                body = Regex.Replace(body, "\\s*,\\s*", ",", RegexOptions.Compiled | RegexOptions.ECMAScript);

                // remove whitespace beside of semicolon
                body = Regex.Replace(body, "\\s*;\\s*", ";", RegexOptions.Compiled | RegexOptions.ECMAScript);

                // remove newline after keywords
                body = Regex.Replace(
                    body,
                    "\\r\\n(?<=\\b(abstract|boolean|break|byte|case|catch|char|class|const|continue|default|delete|do|double|else|extends|false|final|finally|float|for|function|goto|if|implements|import|in|instanceof|int|interface|long|native|new|null|package|private|protected|public|return|short|static|super|switch|synchronized|this|throw|throws|transient|true|try|typeof|var|void|while|with)\\r\\n)",
                    " ",
                    RegexOptions.Compiled | RegexOptions.ECMAScript);

                // remove all newline
                // body = Regex.Replace(body, "\\r\\n", "", RegexOptions.Compiled | RegexOptions.ECMAScript);

                // restore marked strings and regular expressions
                for (var i = 0; i < strs.Count; i++)
                {
                    var sb = new StringBuilder();
                    sb.Append("_____STRINGREGEX_");
                    sb.Append(i.ToString());
                    sb.Append("_STRINGREGEX_____");
                    body = Regex.Replace(body, sb.ToString(), strs[i]);
                }
            }
            else
            {
                // string[] lines = body.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                // StringBuilder sb = new StringBuilder();
                // foreach (string line in lines)
                // {
                // string s = line.Trim();
                // if (s.Length > 0 && !s.StartsWith("//"))
                // sb.AppendLine(s.Trim());
                // }

                // body = sb.ToString();
                body = Regex.Replace(body, @"^[\s]+|[ \f\r\t\v]+$", String.Empty);
                body = Regex.Replace(body, @"([+-])\n\1", "$1 $1");
                body = Regex.Replace(body, @"([^+-][+-])\n", "$1");
                body = Regex.Replace(body, @"([^+]) ?(\+)", "$1$2");
                body = Regex.Replace(body, @"(\+) ?([^+])", "$1$2");
                body = Regex.Replace(body, @"([^-]) ?(\-)", "$1$2");
                body = Regex.Replace(body, @"(\-) ?([^-])", "$1$2");
                body = Regex.Replace(body, @"\n([{}()[\],<>/*%&|^!~?:=.;+-])", "$1");
                body = Regex.Replace(body, @"(\W(if|while|for)\([^{]*?\))\n", "$1");
                body = Regex.Replace(body, @"(\W(if|while|for)\([^{]*?\))((if|while|for)\([^{]*?\))\n", "$1$3");
                body = Regex.Replace(body, @"([;}]else)\n", "$1 ");
                body = Regex.Replace(
                    body, @"(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,}(?=&nbsp;)|(?<=&ndsp;)\s{2,}(?=[<])", String.Empty);
            }

            return body;
        }

        #endregion

        // #region Compression

        // private const string GZIP = "gzip";
        // private const string DEFLATE = "deflate";

        // private static void Compress(HttpContext context)
        // {
        // if (context.Request.UserAgent != null && context.Request.UserAgent.Contains("MSIE 6"))
        // return;

        // if (IsEncodingAccepted(GZIP))
        // {
        // context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
        // SetEncoding(GZIP);
        // }
        // else if (IsEncodingAccepted(DEFLATE))
        // {
        // context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
        // SetEncoding(DEFLATE);
        // }
        // }

        ///// <summary>
        ///// Checks the request headers to see if the specified
        ///// encoding is accepted by the client.
        ///// </summary>
        // private static bool IsEncodingAccepted(string encoding)
        // {
        // return HttpContext.Current.Request.Headers["Accept-encoding"] != null && HttpContext.Current.Request.Headers["Accept-encoding"].Contains(encoding);
        // }

        ///// <summary>
        ///// Adds the specified encoding to the response headers.
        ///// </summary>
        ///// <param name="encoding"></param>
        // private static void SetEncoding(string encoding)
        // {
        // HttpContext.Current.Response.AppendHeader("Content-encoding", encoding);
        // }

        // #endregion
    }
}