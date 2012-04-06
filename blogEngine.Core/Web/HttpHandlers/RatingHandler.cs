namespace BlogEngine.Core.Web.HttpHandlers
{
    using System;
    using System.Web;

    /// <summary>
    /// Receives and records all ratings comming in from the rating control.
    /// </summary>
    public class RatingHandler : IHttpHandler
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
        /// Enables processing of HTTP Web requests by a custom HttpHandler that 
        ///     implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
        /// </summary>
        /// <param name="context">
        /// An <see cref="T:System.Web.HttpContext"></see> 
        ///     object that provides references to the intrinsic server objects 
        ///     (for example, Request, Response, Session, and Server) used to service HTTP requests.
        /// </param>
        public void ProcessRequest(HttpContext context)
        {
            var id = context.Request.QueryString["id"];
            var rating = context.Request.QueryString["rating"];
            int rate;
            if (rating != null && int.TryParse(rating, out rate))
            {
                if (id != null && id.Length == 36 && rate > 0 && rate < 6)
                {
                    var hasRated = HasRated(id);

                    if (hasRated)
                    {
                        context.Response.Write(string.Format("{0}HASRATED", rate));
                        context.Response.End();
                    }

                    var post = Post.GetPost(new Guid(id));
                    post.Rate(rate);

                    SetCookie(id, context);
                    context.Response.Write(string.Format("{0}OK", rate));
                    context.Response.End();
                }
            }

            context.Response.Write("FAIL");
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified post id has rated.
        /// </summary>
        /// <param name="postId">The post id.</param>
        /// <returns>
        ///     <c>true</c> if the specified post id has rated; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasRated(string postId)
        {
            if (HttpContext.Current.Request.Cookies["rating"] != null)
            {
                var cookie = HttpContext.Current.Request.Cookies["rating"];
                if (cookie != null)
                {
                    return cookie.Value.Contains(postId);
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="id">The cookie id.</param>
        /// <param name="context">The context.</param>
        private static void SetCookie(string id, HttpContext context)
        {
            var cookie = context.Request.Cookies["rating"] ?? new HttpCookie("rating");

            cookie.Expires = DateTime.Now.AddYears(2);
            cookie.Value += id;
            context.Response.Cookies.Add(cookie);
        }

        #endregion
    }
}