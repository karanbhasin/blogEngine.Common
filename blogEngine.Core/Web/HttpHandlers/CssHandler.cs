namespace BlogEngine.Core.Web.HttpHandlers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Security;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;

    using BlogEngine.Core.Web.HttpModules;

    /// <summary>
    /// Removes whitespace in all stylesheets added to the 
    ///     header of the HTML document in site.master.
    /// </summary>
    public class CssHandler : IHttpHandler
    {
        #region Events

        /// <summary>
        ///     Occurs when the requested file does not exist;
        /// </summary>
        public static event EventHandler<EventArgs> BadRequest;

        /// <summary>
        ///     Occurs when a file is served;
        /// </summary>
        public static event EventHandler<EventArgs> Served;

        /// <summary>
        ///     Occurs when the requested file does not exist;
        /// </summary>
        public static event EventHandler<EventArgs> Serving;

        #endregion

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
            if (!string.IsNullOrEmpty(context.Request.QueryString["name"]))
            {
                var fileName = context.Request.QueryString["name"].Replace(
                    BlogSettings.Instance.Version(), string.Empty);

                OnServing(fileName);

                // Check if a .css file was requested
                if (!fileName.EndsWith("css", StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityException("Invalid CSS file extension");
                }

                // In cache?
                // Not found in cache, let's load it up
                // Found in cache
                var css = context.Cache[context.Request.RawUrl] == null
                                 ? (fileName.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                        ? RetrieveRemoteCss(fileName)
                                        : RetrieveLocalCss(fileName))
                                 : (string)context.Cache[context.Request.RawUrl];

                // Make sure css isn't empty
                if (!string.IsNullOrEmpty(css))
                {
                    // Configure response headers
                    SetHeaders(css.GetHashCode(), context);

                    context.Response.Write(css);

                    // Check if we should compress content
                    if (BlogSettings.Instance.EnableHttpCompression)
                    {
                        CompressionModule.CompressResponse(context);
                    }

                    OnServed(fileName);
                }
                else
                {
                    OnBadRequest(fileName);
                    context.Response.Status = "404 Bad Request";
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Called when [bad request].
        /// </summary>
        /// <param name="file">The file name.</param>
        private static void OnBadRequest(string file)
        {
            if (BadRequest != null)
            {
                BadRequest(file, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when [served].
        /// </summary>
        /// <param name="file">The file name.</param>
        private static void OnServed(string file)
        {
            if (Served != null)
            {
                Served(file, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when [serving].
        /// </summary>
        /// <param name="file">The file name.</param>
        private static void OnServing(string file)
        {
            if (Serving != null)
            {
                Serving(file, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Retrieves the local CSS from the disk
        /// </summary>
        /// <param name="file">
        /// The file name.
        /// </param>
        /// <returns>
        /// The retrieve local css.
        /// </returns>
        private static string RetrieveLocalCss(string file)
        {
            var path = HttpContext.Current.Server.MapPath(file);
            try
            {
                string css;
                using (var reader = new StreamReader(path))
                {
                    // load CSS content
                    css = reader.ReadToEnd();

                    // Optimize CSS content
                    css = StripWhitespace(css);

                    // Insert into cache
                    HttpContext.Current.Cache.Insert(HttpContext.Current.Request.RawUrl, css, new CacheDependency(path));
                }

                return css;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Retrieves the specified remote CSS using a WebClient.
        /// </summary>
        /// <param name="file">
        /// The remote URL
        /// </param>
        /// <returns>
        /// The retrieve remote css.
        /// </returns>
        private static string RetrieveRemoteCss(string file)
        {
            try
            {
                var url = new Uri(file, UriKind.Absolute);
                string css;
                using (var client = new WebClient())
                {
                    // Load CSS content
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                    css = client.DownloadString(url);

                    // Optimize CSS content
                    css = StripWhitespace(css);

                    // Insert into cache
                    HttpContext.Current.Cache.Insert(
                        HttpContext.Current.Request.RawUrl, 
                        css, 
                        null, 
                        Cache.NoAbsoluteExpiration, 
                        new TimeSpan(3, 0, 0, 0));
                }

                return css;
            }
            catch (SocketException)
            {
                return string.Empty;
            }
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
            context.Response.ContentType = "text/css";
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
        /// <returns>
        /// The strip whitespace.
        /// </returns>
        private static string StripWhitespace(string body)
        {
            body = body.Replace("  ", " ");
            body = body.Replace(Environment.NewLine, String.Empty);
            body = body.Replace("\t", string.Empty);
            body = body.Replace(" {", "{");
            body = body.Replace(" :", ":");
            body = body.Replace(": ", ":");
            body = body.Replace(", ", ",");
            body = body.Replace("; ", ";");
            body = body.Replace(";}", "}");

            // sometimes found when retrieving CSS remotely
            body = body.Replace(@"?", string.Empty);

            // body = Regex.Replace(body, @"/\*[^\*]*\*+([^/\*]*\*+)*/", "$1");
            body = Regex.Replace(
                body, @"(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,}(?=&nbsp;)|(?<=&ndsp;)\s{2,}(?=[<])", String.Empty);

            // Remove comments from CSS
            body = Regex.Replace(body, @"/\*[\d\D]*?\*/", string.Empty);

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