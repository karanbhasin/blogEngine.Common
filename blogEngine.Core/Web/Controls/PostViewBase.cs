namespace BlogEngine.Core.Web.Controls
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>
    /// The PostView.ascx that is located in the themes folder
    ///     has to inherit from this class.
    ///     <remarks>
    /// It provides the basic functionaly needed to display a post.
    ///     </remarks>
    /// </summary>
    public class PostViewBase : UserControl
    {
        #region Constants and Fields

        /// <summary>
        /// The body regex.
        /// </summary>
        private static readonly Regex BodyRegex = new Regex(
            @"\[UserControl:(.*?)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="PostViewBase"/> class.
        /// </summary>
        public PostViewBase()
        {
            this.Location = ServingLocation.None;
            this.ContentBy = ServingContentBy.Unspecified;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the body of the post. Important: use this instead of Post.Content.
        /// </summary>
        public string Body
        {
            get
            {
                var body = this.Post.Content;

                if (this.ShowExcerpt)
                {
                    var link = string.Format(" <a href=\"{0}\">[{1}]</a>", this.Post.RelativeLink, Utils.Translate("more"));

                    if (!string.IsNullOrEmpty(this.Post.Description))
                    {
                        body = this.Post.Description.Replace(Environment.NewLine, "<br />") + link;
                    }
                    else
                    {
                        body = Utils.StripHtml(this.Post.Content);
                        if (body.Length > this.DescriptionCharacters)
                        {
                            body = string.Format("{0}...{1}", body.Substring(0, this.DescriptionCharacters), link);
                        }
                    }
                }

                var arg = new ServingEventArgs(body, this.Location, this.ContentBy);
                Post.OnServing(this.Post, arg);

                if (arg.Cancel)
                {
                    if (arg.Location == ServingLocation.SinglePost)
                    {
                        this.Response.Redirect("~/error404.aspx", true);
                    }
                    else
                    {
                        this.Visible = false;
                    }
                }

                return arg.Body ?? string.Empty;
            }
        }

        /// <summary>
        ///     Gets the comment feed link.
        /// </summary>
        /// <value>The comment feed.</value>
        public string CommentFeed
        {
            get
            {
                return this.Post.RelativeLink.Replace("/post/", "/post/feed/");
            }
        }

        /// <summary>
        ///     Gets or sets the criteria by which the content is being served (by tag, category, author, etc).
        /// </summary>
        public ServingContentBy ContentBy { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating how many characters should be shown of the description.
        /// </summary>
        public int DescriptionCharacters { get; set; }

        /// <summary>
        ///     Gets or sets the index of the post in a list of posts displayed
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///     Gets or sets the location where the serving takes place.
        /// </summary>
        public ServingLocation Location { get; set; }

        /// <summary>
        ///     Gets or sets the Post object that is displayed through the PostView.ascx control.
        /// </summary>
        /// <value>The Post object that has to be displayed.</value>
        public virtual Post Post
        {
            get
            {
                return (Post)this.ViewState["Post"];
            }

            set
            {
                this.ViewState["Post"] = value;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether or not to show the entire post or just the excerpt/description.
        /// </summary>
        public bool ShowExcerpt { get; set; }

        /// <summary>
        ///     Gets an Edit and Delete link to any authenticated user.
        /// </summary>
        protected virtual string AdminLinks
        {
            get
            {
                if (this.Page.User.IsInRole(BlogSettings.Instance.AdministratorRole) ||
                    this.Page.User.Identity.Name.Equals(this.Post.Author, StringComparison.OrdinalIgnoreCase))
                {
                    var confirmDelete = string.Format(
                        CultureInfo.InvariantCulture,
                        Utils.Translate("areYouSure"),
                        Utils.Translate("delete").ToLowerInvariant(),
                        Utils.Translate("thePost"));
                    var sb = new StringBuilder();

                    if (this.Post.NotApprovedComments.Count > 0 &&
                        BlogSettings.Instance.ModerationType != BlogSettings.Moderation.Disqus)
                    {
                        sb.AppendFormat(
                            CultureInfo.InvariantCulture,
                            "<a href=\"{0}\">{1} ({2})</a> | ",
                            this.Post.RelativeLink,
                            Utils.Translate("unapprovedcomments"),
                            this.Post.NotApprovedComments.Count);
                        sb.AppendFormat(
                            CultureInfo.InvariantCulture,
                            "<a href=\"{0}\">{1}</a> | ",
                            this.Post.RelativeLink + "?approveallcomments=true",
                            Utils.Translate("approveallcomments"));
                    }

                    sb.AppendFormat(
                        CultureInfo.InvariantCulture,
                        "<a href=\"{0}\">{1}</a> | ",
                        Utils.AbsoluteWebRoot + "admin/Pages/Add_entry.aspx?id=" + this.Post.Id,
                        Utils.Translate("edit"));
                    sb.AppendFormat(
                        CultureInfo.InvariantCulture,
                        "<a href=\"javascript:void(0);\" onclick=\"if (confirm('{2}')) location.href='{0}?deletepost={1}'\">{3}</a> | ",
                        this.Post.RelativeLink,
                        this.Post.Id,
                        confirmDelete,
                        Utils.Translate("delete"));
                    return sb.ToString();
                }

                return string.Empty;
            }
        }

        /// <summary>
        ///     Gets the rating.
        /// Enable visitors to rate the post.
        /// </summary>
        protected virtual string Rating
        {
            get
            {
                if (!BlogSettings.Instance.EnableRating)
                {
                    return string.Empty;
                }

                // string script = "<div id=\"rating_{0}\"></div><script type=\"text/javascript\">BlogEngine.showRating('{0}',{1},{2});</script>";
                const string Script = "<div class=\"ratingcontainer\" style=\"visibility:hidden\">{0}|{1}|{2}</div>";
                return string.Format(
                    Script,
                    this.Post.Id,
                    this.Post.Raters,
                    this.Post.Rating.ToString("#.0", CultureInfo.InvariantCulture));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Displays the Post's categories seperated by the specified string.
        /// </summary>
        /// <param name="separator">
        /// The separator.
        /// </param>
        /// <returns>
        /// The category links.
        /// </returns>
        protected virtual string CategoryLinks(string separator)
        {
            var keywords = new string[this.Post.Categories.Count];
            const string Link = "<a href=\"{0}\">{1}</a>";
            for (var i = 0; i < this.Post.Categories.Count; i++)
            {
                var c = Category.GetCategory(this.Post.Categories[i].Id);
                if (c != null)
                {
                    keywords[i] = string.Format(CultureInfo.InvariantCulture, Link, c.RelativeLink, c.Title);
                }
            }

            return string.Join(separator, keywords);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!this.Post.Visible && !this.Page.User.Identity.IsAuthenticated)
            {
                this.Visible = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        /// <remarks>
        /// Lets process our .Body content and build up our controls collection
        /// inside the 'BodyContent' placeholder.
        /// User controls are insterted into the blog in the following format..
        /// [UserControl:~/path/usercontrol.ascx]
        /// TODO : Expose user control parameters.
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Used to track where we are in the 'Body' as we parse it.
            var currentPosition = 0;
            var content = this.Body;
            var bodyContent = (PlaceHolder)this.FindControl("BodyContent");
            if (bodyContent == null)
            {
                // We have no placeholder so we assume this is an old style <% =Body %> theme and do nothing.
            }
            else
            {
                var matches = BodyRegex.Matches(content);

                foreach (Match match in matches)
                {
                    // Add literal for content before custom tag should it exist.
                    if (match.Index > currentPosition)
                    {
                        bodyContent.Controls.Add(
                            new LiteralControl(content.Substring(currentPosition, match.Index - currentPosition)));
                    }

                    // Now lets add our user control.
                    try
                    {
                        var all = match.Groups[1].Value.Trim();
                        Control usercontrol;

                        if (!all.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase))
                        {
                            var index = all.IndexOf(".ascx", StringComparison.OrdinalIgnoreCase) + 5;
                            usercontrol = this.LoadControl(all.Substring(0, index));

                            var parameters = this.Server.HtmlDecode(all.Substring(index));
                            var type = usercontrol.GetType();
                            var paramCollection = parameters.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var param in paramCollection)
                            {
                                var name = param.Split('=')[0].Trim();
                                var value = param.Split('=')[1].Trim();
                                var property = type.GetProperty(name);
                                property.SetValue(
                                    usercontrol,
                                    Convert.ChangeType(value, property.PropertyType, CultureInfo.InvariantCulture),
                                    null);
                            }
                        }
                        else
                        {
                            usercontrol = this.LoadControl(all);
                        }

                        bodyContent.Controls.Add(usercontrol);

                        // Now we will update our position.
                        // currentPosition = myMatch.Index + myMatch.Groups[0].Length;
                    }
                    catch (Exception)
                    {
                        // Whoopss, can't load that control so lets output something that tells the developer that theres a problem.
                        bodyContent.Controls.Add(
                            new LiteralControl(string.Format("ERROR - UNABLE TO LOAD CONTROL : {0}", match.Groups[1].Value)));
                    }

                    currentPosition = match.Index + match.Groups[0].Length;
                }

                // Finally we add any trailing static text.
                bodyContent.Controls.Add(
                    new LiteralControl(content.Substring(currentPosition, content.Length - currentPosition)));
            }
        }

        /// <summary>
        /// Displays the Post's tags seperated by the specified string.
        /// </summary>
        /// <param name="separator">
        /// The separator.
        /// </param>
        /// <returns>
        /// The tag links.
        /// </returns>
        protected virtual string TagLinks(string separator)
        {
            if (this.Post.Tags.Count == 0)
            {
                return null;
            }

            var tags = new string[this.Post.Tags.Count];
            const string Link = "<a href=\"{0}/{1}\" rel=\"tag\">{2}</a>";
            var path = Utils.RelativeWebRoot + "?tag=";
            for (var i = 0; i < this.Post.Tags.Count; i++)
            {
                var tag = this.Post.Tags[i];
                tags[i] = string.Format(
                    CultureInfo.InvariantCulture, Link, path, HttpUtility.UrlEncode(tag), HttpUtility.HtmlEncode(tag));
            }

            return string.Join(separator, tags);
        }

        #endregion
    }
}