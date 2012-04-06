namespace BlogEngine.Core.Web.Controls
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    /// <summary>
    /// All pages in the custom themes as well as pre-defined pages in the root
    ///     must inherit from this class.
    /// </summary>
    /// <remarks>
    /// The class is responsible for assigning the theme to all
    ///     derived pages as well as adding RSS, RSD, tracking script
    ///     and a whole lot more.
    /// </remarks>
    public abstract class BlogBasePage : Page
    {
        #region Constants and Fields

        /// <summary>
        /// The theme.
        /// </summary>
        private readonly string theme = BlogSettings.Instance.Theme;

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the generic link to the header.
        /// </summary>
        /// <param name="relation">
        /// The relation string.
        /// </param>
        /// <param name="title">
        /// The title string.
        /// </param>
        /// <param name="href">
        /// The href string.
        /// </param>
        public virtual void AddGenericLink(string relation, string title, string href)
        {
            using (var link = new HtmlLink())
            {
                link.Attributes["rel"] = relation;
                link.Attributes["title"] = title;
                link.Attributes["href"] = href;
                this.Page.Header.Controls.Add(link);
            }
        }

        /// <summary>
        /// Adds the generic link to the header.
        /// </summary>
        /// <param name="type">
        /// The type string.
        /// </param>
        /// <param name="relation">
        /// The relation string.
        /// </param>
        /// <param name="title">
        /// The title string.
        /// </param>
        /// <param name="href">
        /// The href string.
        /// </param>
        public virtual void AddGenericLink(string type, string relation, string title, string href)
        {
            var link = new HtmlLink();
            link.Attributes["type"] = type;
            link.Attributes["rel"] = relation;
            link.Attributes["title"] = title;
            link.Attributes["href"] = href;
            this.Page.Header.Controls.Add(link);
        }

        /// <summary>
        /// Adds a JavaScript reference to the HTML head tag.
        /// </summary>
        /// <param name="url">
        /// The url string.
        /// </param>
        /// <param name="placeInBottom">
        /// The place In Bottom.
        /// </param>
        /// <param name="addDeferAttribute">
        /// The add Defer Attribute.
        /// </param>
        public virtual void AddJavaScriptInclude(string url, bool placeInBottom, bool addDeferAttribute)
        {
            if (placeInBottom)
            {
                var script = string.Format("<script type=\"text/javascript\"{0} src=\"{1}\"></script>", (addDeferAttribute ? " defer=\"defer\"" : string.Empty), this.ResolveScriptUrl(url));
                this.ClientScript.RegisterStartupScript(this.GetType(), url.GetHashCode().ToString(), script);
            }
            else
            {
                var script = new HtmlGenericControl("script");
                script.Attributes["type"] = "text/javascript";
                script.Attributes["src"] = this.ResolveScriptUrl(url);
                if (addDeferAttribute)
                {
                    script.Attributes["defer"] = "defer";
                }

                this.Page.Header.Controls.Add(script);
            }
        }

        /// <summary>
        /// Adds a Stylesheet reference to the HTML head tag.
        /// </summary>
        /// <param name="url">
        /// The relative URL.
        /// </param>
        /// <param name="insertAtFront">
        /// If true, inserts in beginning of HTML head tag.
        /// </param>
        public virtual void AddStylesheetInclude(string url, bool insertAtFront)
        {
            var link = new HtmlLink();
            link.Attributes["type"] = "text/css";
            link.Attributes["href"] = url;
            link.Attributes["rel"] = "stylesheet";

            if (insertAtFront)
            {
                this.Page.Header.Controls.AddAt(0, link);
            }
            else
            {
                this.Page.Header.Controls.Add(link);
            }
        }

        /// <summary>
        /// Adds a Stylesheet reference to the HTML head tag.
        /// </summary>
        /// <param name="url">
        /// The relative URL.
        /// </param>
        public virtual void AddStylesheetInclude(string url)
        {
            this.AddStylesheetInclude(url, false);
        }

        /// <summary>
        /// Resolves the script URL.
        /// </summary>
        /// <param name="url">
        /// The URL string.
        /// </param>
        /// <returns>
        /// The resolve script url.
        /// </returns>
        public virtual string ResolveScriptUrl(string url)
        {
            return string.Format("{0}js.axd?path={1}", Utils.RelativeWebRoot, HttpUtility.UrlEncode(url));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds code to the HTML head section.
        /// </summary>
        protected virtual void AddCustomCodeToHead()
        {
            var code = string.Format(
                CultureInfo.InvariantCulture,
                "{0}<!-- Start custom code -->{0}{1}{0}<!-- End custom code -->{0}",
                Environment.NewLine,
                BlogSettings.Instance.HtmlHeader);
            var control = new LiteralControl(code);
            this.Page.Header.Controls.Add(control);
        }

        /// <summary>
        /// Adds the default stylesheet language
        /// </summary>
        protected virtual void AddDefaultLanguages()
        {
            this.Response.AppendHeader("Content-Style-Type", "text/css");
            this.Response.AppendHeader("Content-Script-Type", "text/javascript");
        }

        /// <summary>
        /// Add global style sheets before any custom css
        /// </summary>
        protected virtual void AddGlobalStyles()
        {
            // add styles in the ~/Styles folder to the page header
            var s = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "Styles");
            var fileEntries = Directory.GetFiles(s);
            foreach (var fileName in
                fileEntries.Where(fileName => fileName.EndsWith(".css", StringComparison.OrdinalIgnoreCase)))
            {
                this.AddStylesheetInclude(
                    string.Format("{0}Styles/{1}", Utils.RelativeWebRoot, Utils.ExtractFileNameFromPath(fileName)), true);
            }
        }

        /// <summary>
        /// Add java scripts from /Scripts and custom theme to any post or page
        ///     Ignore jQuery document file as exception, it is used for VS only
        /// </summary>
        protected virtual void AddJavaScripts()
        {
            // add scripts in the ~/Scripts folder to the page header
            var s = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "Scripts");
            var fileEntries = Directory.GetFiles(s);
            foreach (var fileName in
                fileEntries.Where(fileName => fileName.EndsWith(".js", StringComparison.OrdinalIgnoreCase) && !fileName.EndsWith("-vsdoc.js", StringComparison.OrdinalIgnoreCase)))
            {
                this.AddJavaScriptInclude(
                    string.Format("{0}Scripts/{1}", Utils.RelativeWebRoot, Utils.ExtractFileNameFromPath(fileName)), true, true);
            }

            // add scripts in the custom theme folder
            s = Path.Combine(HttpContext.Current.Server.MapPath("~/"), string.Format("themes/{0}", this.theme));
            fileEntries = Directory.GetFiles(s);
            foreach (var fileName in
                fileEntries.Where(fileName => fileName.EndsWith(".js", StringComparison.OrdinalIgnoreCase)))
            {
                this.AddJavaScriptInclude(
                    string.Format("{0}themes/{1}/{2}", Utils.RelativeWebRoot, this.theme, Utils.ExtractFileNameFromPath(fileName)),
                    true,
                    true);
            }
        }

        /// <summary>
        /// Adds the localization keys to JavaScript for use globally.
        /// </summary>
        protected virtual void AddLocalizationKeys()
        {
            var sb = new StringBuilder();
            sb.Append("function registerVariables(){");
            sb.AppendFormat("BlogEngine.webRoot='{0}';", Utils.RelativeWebRoot);
            sb.AppendFormat("BlogEngine.i18n.hasRated='{0}';", Utils.Translate("youAlreadyRated").Replace("'", "\\'"));
            sb.AppendFormat(
                "BlogEngine.i18n.savingTheComment='{0}';", Utils.Translate("savingTheComment").Replace("'", "\\'"));
            sb.AppendFormat("BlogEngine.i18n.comments='{0}';", Utils.Translate("comments").Replace("'", "\\'"));
            sb.AppendFormat(
                "BlogEngine.i18n.commentWasSaved='{0}';", Utils.Translate("commentWasSaved").Replace("'", "\\'"));
            sb.AppendFormat(
                "BlogEngine.i18n.commentWaitingModeration='{0}';",
                Utils.Translate("commentWaitingModeration").Replace("'", "\\'"));
            sb.AppendFormat("BlogEngine.i18n.cancel='{0}';", Utils.Translate("cancel").Replace("'", "\\'"));
            sb.AppendFormat("BlogEngine.i18n.filter='{0}';", Utils.Translate("filter").Replace("'", "\\'"));
            sb.AppendFormat(
                "BlogEngine.i18n.apmlDescription='{0}';", Utils.Translate("filterByApmlDescription").Replace("'", "\\'"));
            sb.AppendFormat(
                "BlogEngine.i18n.beTheFirstToRate='{0}';", Utils.Translate("beTheFirstToRate").Replace("'", "\\'"));
            sb.AppendFormat(
                "BlogEngine.i18n.currentlyRated='{0}';", Utils.Translate("currentlyRated").Replace("'", "\\'"));
            sb.AppendFormat(
                "BlogEngine.i18n.ratingHasBeenRegistered='{0}';",
                Utils.Translate("ratingHasBeenRegistered").Replace("'", "\\'"));
            sb.AppendFormat(
                "BlogEngine.i18n.rateThisXStars='{0}';", Utils.Translate("rateThisXStars").Replace("'", "\\'"));

            sb.Append("};");

            this.ClientScript.RegisterStartupScript(this.GetType(), "registerVariables", sb.ToString(), true);
        }

        /// <summary>
        /// Adds the content-type meta tag to the header.
        /// </summary>
        protected virtual void AddMetaContentType()
        {
            var meta = new HtmlMeta
                {
                    HttpEquiv = "content-type",
                    Content =
                        string.Format(
                            "{0}; charset={1}", this.Response.ContentType, this.Response.ContentEncoding.HeaderName)
                };
            this.Page.Header.Controls.Add(meta);
        }

        /// <summary>
        /// Add a meta tag to the page's header.
        /// </summary>
        /// <param name="name">
        /// The tag name.
        /// </param>
        /// <param name="value">
        /// The tag value.
        /// </param>
        protected virtual void AddMetaTag(string name, string value)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
            {
                return;
            }

            var meta = new HtmlMeta { Name = name, Content = value };
            this.Page.Header.Controls.Add(meta);
        }

        /// <summary>
        /// Adds a JavaScript to the bottom of the page at runtime.
        /// </summary>
        /// <remarks>
        /// You must add the script tags to the BlogSettings.Instance.TrackingScript.
        /// </remarks>
        protected virtual void AddTrackingScript()
        {
            var sb = new StringBuilder();

            if (BlogSettings.Instance.ModerationType == BlogSettings.Moderation.Disqus)
            {
                sb.Append("<script type=\"text/javascript\"> \n");
                sb.Append("//<![CDATA[ \n");
                sb.Append("(function() { ");
                sb.Append("var links = document.getElementsByTagName('a'); ");
                sb.Append("var query = '?'; ");
                sb.Append("for(var i = 0; i < links.length; i++) { ");
                sb.Append("if(links[i].href.indexOf('#disqus_thread') >= 0) { ");
                sb.Append("query += 'url' + i + '=' + encodeURIComponent(links[i].href) + '&'; ");
                sb.Append("}}");
                sb.Append("document.write('<script charset=\"utf-8\" type=\"text/javascript\" src=\"http://disqus.com/forums/");
                sb.Append(BlogSettings.Instance.DisqusWebsiteName);
                sb.Append("/get_num_replies.js' + query + '\"></' + 'script>'); ");
                sb.Append("})(); \n");
                sb.Append("//]]> \n");
                sb.Append("</script> \n");
            }

            if (!string.IsNullOrEmpty(BlogSettings.Instance.TrackingScript))
            {
                sb.Append(BlogSettings.Instance.TrackingScript);
            }

            var s = sb.ToString();
            if (!string.IsNullOrEmpty(s))
            {
                this.ClientScript.RegisterStartupScript(this.GetType(), "tracking", string.Format("\n{0}", s), false);
            }
        }

        /// <summary>
        /// Finds all stylesheets in the header and changes the 
        ///     path so it points to css.axd which removes the whitespace.
        /// </summary>
        protected virtual void CompressCss()
        {
            if (this.Request.QueryString["theme"] != null)
            {
                return;
            }

            foreach (Control control in this.Page.Header.Controls)
            {
                var c = control as HtmlControl;
                if (c == null || c.Attributes["type"] == null ||
                    !c.Attributes["type"].Equals("text/css", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (c.Attributes["href"].StartsWith("http://"))
                {
                    continue;
                }
                
                var url = string.Format("{0}themes/{1}/css.axd?name={2}", Utils.RelativeWebRoot, this.theme, c.Attributes["href"]);
                c.Attributes["href"] = url.Replace(".css", string.Format("{0}.css", BlogSettings.Instance.Version()));
                c.EnableViewState = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.TemplateControl.Error"></see> event.
        /// </summary>
        /// <param name="e">
        /// An <see cref="T:System.EventArgs"></see> that contains the event data.
        /// </param>
        protected override void OnError(EventArgs e)
        {
            var ctx = HttpContext.Current;
            var exception = ctx.Server.GetLastError();

            if (exception != null && exception.Message.Contains("callback"))
            {
                // This is a robot spam attack so we send it a 404 status to make it go away.
                ctx.Response.StatusCode = 404;
                ctx.Server.ClearError();
                Comment.OnSpamAttack();
            }

            base.OnError(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// Adds links and javascript to the HTML header tag.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!this.Page.IsCallback)
            {
                // Links
                this.AddGenericLink("contents", "Archive", string.Format("{0}archive.aspx", Utils.RelativeWebRoot));
                this.AddGenericLink("start", BlogSettings.Instance.Name, Utils.RelativeWebRoot);
                this.AddGenericLink("application/rdf+xml", "meta", "SIOC", string.Format("{0}sioc.axd", Utils.AbsoluteWebRoot));
                this.AddGenericLink("application/apml+xml", "meta", "APML", string.Format("{0}apml.axd", Utils.AbsoluteWebRoot));
                this.AddGenericLink("application/rdf+xml", "meta", "FOAF", string.Format("{0}foaf.axd", Utils.AbsoluteWebRoot));

                if (string.IsNullOrEmpty(BlogSettings.Instance.AlternateFeedUrl))
                {
                    this.AddGenericLink(
                        "application/rss+xml",
                        "alternate",
                        string.Format("{0} (RSS)", BlogSettings.Instance.Name),
                        string.Format("{0}?format=rss", Utils.FeedUrl));
                    this.AddGenericLink(
                        "application/atom+xml",
                        "alternate",
                        string.Format("{0} (ATOM)", BlogSettings.Instance.Name),
                        string.Format("{0}?format=atom", Utils.FeedUrl));
                }
                else
                {
                    this.AddGenericLink("application/rss+xml", "alternate", BlogSettings.Instance.Name, Utils.FeedUrl);
                }

                this.AddGenericLink("application/rsd+xml", "edituri", "RSD", string.Format("{0}rsd.axd", Utils.AbsoluteWebRoot));

                this.AddMetaContentType();

                this.AddDefaultLanguages();

                this.AddLocalizationKeys();

                this.AddGlobalStyles();

                this.AddJavaScripts();

                if (BlogSettings.Instance.EnableOpenSearch)
                {
                    this.AddGenericLink(
                        "application/opensearchdescription+xml",
                        "search",
                        BlogSettings.Instance.Name,
                        string.Format("{0}opensearch.axd", Utils.AbsoluteWebRoot));
                }

                if (!string.IsNullOrEmpty(BlogSettings.Instance.HtmlHeader))
                {
                    this.AddCustomCodeToHead();
                }

                this.AddTrackingScript();
            }

            if (this.User.IsInRole(BlogSettings.Instance.AdministratorRole))
            {
                this.AddJavaScriptInclude(string.Format("{0}admin/widget.js", Utils.RelativeWebRoot), true, true);
            }

            if (BlogSettings.Instance.RemoveWhitespaceInStyleSheets)
            {
                this.CompressCss();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Page.PreInit"/> event at the beginning of page initialization.
        /// Assignes the selected theme to the pages.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnPreInit(EventArgs e)
        {
            // blog set as private - redirect to login if not authenticated
            if (!this.Page.User.Identity.IsAuthenticated && BlogSettings.Instance.RequireLoginToViewPosts)
            {
                this.Response.Redirect(string.Format("{0}Account/login.aspx", Utils.RelativeWebRoot));
            }

            this.MasterPageFile = string.Format("{0}themes/{1}/site.master", Utils.RelativeWebRoot, this.theme);

            base.OnPreInit(e);

            if (this.Page.IsPostBack || string.IsNullOrEmpty(this.Request.QueryString["deletepost"]))
            {
                return;
            }

            if (!this.Page.User.Identity.IsAuthenticated)
            {
                return;
            }
            
            var post = Post.GetPost(new Guid(this.Request.QueryString["deletepost"]));
            if (!this.Page.User.IsInRole(BlogSettings.Instance.AdministratorRole) &&
                this.Page.User.Identity.Name != post.Author)
            {
                return;
            }
            
            post.Delete();
            post.Save();
            this.Response.Redirect(Utils.RelativeWebRoot);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Page.PreRenderComplete"></see> event after 
        ///     the <see cref="M:System.Web.UI.Page.OnPreRenderComplete(System.EventArgs)"></see> event and before the page is rendered.
        /// </summary>
        /// <param name="e">
        /// An <see cref="T:System.EventArgs"></see> that contains the event data.
        /// </param>
        protected override void OnPreRenderComplete(EventArgs e)
        {
            base.OnPreRenderComplete(e);
            if (BlogSettings.Instance.UseBlogNameInPageTitles)
            {
                this.Page.Title = string.Format("{0} | {1}", BlogSettings.Instance.Name, this.Page.Title);
            }
        }

        /// <summary>
        /// Initializes the <see cref="T:System.Web.UI.HtmlTextWriter"></see> object and calls on the child
        ///     controls of the <see cref="T:System.Web.UI.Page"></see> to render.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="T:System.Web.UI.HtmlTextWriter"></see> that receives the page content.
        /// </param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(new RewriteFormHtmlTextWriter(writer));
        }

        #endregion
    }
}