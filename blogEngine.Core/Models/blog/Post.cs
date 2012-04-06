using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.ComponentModel.DataAnnotations;

using blogEngine.Core.Providers;

namespace blogEngine.Core.Models
{


    /// <summary>
    /// A post is an entry on the blog - a blog post.
    /// </summary>
    [Serializable]
    public class Post : BaseEntity<Guid>, IComparable<Post>
    {
        #region Constants and Fields

        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The categories.
        /// </summary>
        private readonly StateList<Category> categories;

        /// <summary>
        /// The comments.
        /// </summary>
        private readonly List<Comment> comments;

        /// <summary>
        /// The notification emails.
        /// </summary>
        private readonly StateList<string> notificationEmails;

        /// <summary>
        /// The post tags.
        /// </summary>
        private readonly StateList<string> tags;

        private readonly List<Tag> keywords;

        private List<Post> posts = new List<Post>();

        /// <summary>
        /// Nested Posts. In csae of a Photo Blog, where there may be multiple photos, each photo will be in a Photo object. All the photos
        /// in the blog post will be held in the Posts member
        /// </summary>
        public List<Post> Posts {
            get {
                return posts;
            }
            set {
                posts = value;
            }
        }

        /// <summary>
        ///     The author.
        /// </summary>
        private string author;

        /// <summary>
        ///     The content.
        /// </summary>
        private string content;

        /// <summary>
        /// If this is a Photo/Audio/Video, this is the URL where the content is hosted
        /// </summary>
        private string contentURL;

        /// <summary>
        ///     The description.
        /// </summary>
        private string description;

        /// <summary>
        ///     Whether the post is comments enabled.
        /// </summary>
        private bool hasCommentsEnabled;

         /// <summary>
        ///     Whether the post is facebook ready.
        /// </summary>
        private bool showFacebookWidgets;
        

        /// <summary>
        ///     The nested comments.
        /// </summary>
        private List<Comment> nestedComments;

        /// <summary>
        ///     Whether the post is published.
        /// </summary>
        private bool published;

        private DateTime publishDate;

        /// <summary>
        ///     The raters.
        /// </summary>
        private int raters;

        /// <summary>
        ///     The rating.
        /// </summary>
        private float rating;

        /// <summary>
        ///     The slug of the post.
        /// </summary>
        private string slug;

        /// <summary>
        ///     The title.
        /// </summary>
        private string title;

        public int EntityID;

        public PostType PostType = PostType.Blog;
       
        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref = "Post" /> class. 
        ///     The default contstructor assign default values.
        /// </summary>
        public Post()
        {
            this.Id = Guid.NewGuid();
            this.comments = new List<Comment>();
            this.categories = new StateList<Category>();
            this.tags = new StateList<string>();
            this.keywords = new List<Tag>();
            this.notificationEmails = new StateList<string>();
            this.DateCreated = DateTime.Now;
            this.published = true;
            this.hasCommentsEnabled = true;
            this.showFacebookWidgets = false;
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs before a new comment is added.
        /// </summary>
        public static event EventHandler<CancelEventArgs> AddingComment;

        /// <summary>
        ///     Occurs when a comment is added.
        /// </summary>
        public static event EventHandler<EventArgs> CommentAdded;

        /// <summary>
        ///     Occurs when a comment has been removed.
        /// </summary>
        public static event EventHandler<EventArgs> CommentRemoved;

        /// <summary>
        ///     Occurs when a comment is updated.
        /// </summary>
        public static event EventHandler<EventArgs> CommentUpdated;

        /// <summary>
        ///     Occurs when a visitor rates the post.
        /// </summary>
        public static event EventHandler<EventArgs> Rated;

        /// <summary>
        ///     Occurs before comment is removed.
        /// </summary>
        public static event EventHandler<CancelEventArgs> RemovingComment;

        /// <summary>
        ///     Occurs when the post is being served to the output stream.
        /// </summary>
        public static event EventHandler<ServingEventArgs> Serving;

        /// <summary>
        ///     Occurs before a new comment is updated.
        /// </summary>
        public static event EventHandler<CancelEventArgs> UpdatingComment;

        #endregion

        #region Post Properties

        /// <summary>
        ///     Gets a sorted collection of all posts in the blog.
        ///     Sorted by date.
        /// </summary>
        public static List<Post> All
        {
            get
            {
                return BlogProvider.Provider.AllPosts;
            }
        }

        public static List<Post> Blogs {
            get {
                return BlogProvider.Provider.Posts;
            }
        }

        /// <summary>
        ///     Gets a sorted collection of all posts in the blog.
        ///     Sorted by date.
        /// </summary>
        public static List<Post> PhotoPosts {
            get {
                return BlogProvider.Provider.PhotoPosts;
                //return All.Where(x => x.PostType == PostType.Photo).ToList();
            }
        }

        public static void Reload() {
            BlogProvider.Provider.Reload(1);
            BlogProvider.Provider.Reload(2);
        }

        /// <summary>
        ///     Gets the absolute link to the post.
        /// </summary>
        public Uri AbsoluteLink
        {
            get
            {
                return Utils.ConvertToAbsolute(this.RelativeLink);
            }
        }

        /// <summary>
        ///     Gets or sets the Author or the post.
        /// </summary>
        public string Author
        {
            get
            {
                return this.author;
            }

            set
            {
                if (this.author != value)
                {
                    this.MarkChanged("Author");
                }

                this.author = value;
            }
        }

        /// <summary>
        ///     Gets AuthorProfile.
        /// </summary>
        public AuthorProfile AuthorProfile
        {
            get
            {
                return AuthorProfile.GetProfile(this.Author);
            }
        }

        /// <summary>
        ///     Gets an unsorted List of categories.
        /// </summary>
        public StateList<Category> Categories
        {
            get
            {
                return this.categories;
            }
        }

        public List<Tag> Keywords {
            get {
                return this.keywords;
            }
        }

        /// <summary>
        ///     Gets or sets the Content of the post. For Photo, this will be the 
        ///     caption
        /// </summary>
        public string Content
        {
            get
            {
                return this.content;
            }

            set
            {
                if (this.content == value)
                {
                    return;
                }

                this.MarkChanged("Content");
                HttpContext.Current.Cache.Remove("content_" + this.Id);
                this.content = value;
            }
        }

        public string ContentURL {
            get {
                return this.contentURL;
            }

            set {
                if (this.contentURL == value) {
                    return;
                }

                this.MarkChanged("ContentURL");
                this.contentURL = value;
            }
        }

        /// <summary>
        ///     Gets or sets the Description or the post.
        /// </summary>
        [StringLength(100, ErrorMessage = "Description must be under 100 characters")]
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                if (this.description != value)
                {
                    this.MarkChanged("Description");
                }

                this.description = value;
            }
        }

        /// <summary>
        ///     Gets if the Post have been changed.
        /// </summary>
        //public override bool IsChanged
        //{
        //    get
        //    {
        //        if (base.IsChanged)
        //        {
        //            return true;
        //        }

        //        if (this.Categories.IsChanged || this.Tags.IsChanged || this.NotificationEmails.IsChanged)
        //        {
        //            return true;
        //        }

        //        return false;
        //    }
        //}

        /// <summary>
        ///     Gets the next post relative to this one based on time.
        ///     <remarks>
        ///         If this post is the newest, then it returns null.
        ///     </remarks>
        /// </summary>
        public Post Next { get; private set; }

        /// <summary>
        ///     Gets a collection of email addresses that is signed up for 
        ///     comment notification on the specific post.
        /// </summary>
        public StateList<string> NotificationEmails
        {
            get
            {
                return this.notificationEmails;
            }
        }

        /// <summary>
        ///     Gets the absolute permanent link to the post.
        /// </summary>
        public Uri PermaLink
        {
            get
            {
                return new Uri(string.Format("{0}post.aspx?id={1}", Utils.AbsoluteWebRoot, this.Id));
            }
        }

        /// <summary>
        ///     Gets the previous post relative to this one based on time.
        ///     <remarks>
        ///         If this post is the oldest, then it returns null.
        ///     </remarks>
        /// </summary>
        public Post Previous { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the post is published.
        /// </summary>
        public bool Published
        {
            get
            {
                return this.published;
            }

            set
            {
                if (this.published != value)
                {
                    this.MarkChanged("Published");
                }

                this.published = value;
            }
        }

        public DateTime PublishDate {
            get {
                return this.publishDate;
            }

            set {
                if (this.publishDate != value) {
                    this.MarkChanged("PublishDate");
                }

                this.publishDate = value;
            }
        }

        /// <summary>
        ///     Gets or sets the number of raters or the object.
        /// </summary>
        public int Raters
        {
            get
            {
                return this.raters;
            }

            set
            {
                if (this.raters != value)
                {
                    this.MarkChanged("Raters");
                }

                this.raters = value;
            }
        }

        /// <summary>
        ///     Gets or sets the rating or the post.
        /// </summary>
        public float Rating
        {
            get
            {
                return this.rating;
            }

            set
            {
                if (this.rating != value)
                {
                    this.MarkChanged("Rating");
                }

                this.rating = value;
            }
        }

        /// <summary>
        ///     Gets a relative-to-the-site-root path to the post.
        ///     Only for in-site use.
        /// </summary>
        public string RelativeLink
        {
            get
            {
                var theslug = Utils.RemoveIllegalCharacters(this.Slug) + BlogSettings.Instance.FileExtension;

                return BlogSettings.Instance.TimeStampPostLinks
                           ? string.Format(
                               "{0}post/{1}{2}",
                               Utils.RelativeWebRoot,
                               this.DateCreated.ToString("yyyy/MM/dd/", CultureInfo.InvariantCulture),
                               theslug)
                           : string.Format("{0}post/{1}", Utils.RelativeWebRoot, theslug);
            }
        }

        /// <summary>
        ///     Gets or sets the Slug of the Post.
        ///     A Slug is the relative URL used by the posts.
        /// </summary>
        public string Slug
        {
            get
            {
                return string.IsNullOrEmpty(this.slug) ? Utils.RemoveIllegalCharacters(this.Title) : this.slug;
            }

            set
            {
                if (this.slug != value)
                {
                    this.MarkChanged("Slug");
                }

                this.slug = value;
            }
        }

        /// <summary>
        ///     Gets an unsorted collection of tags.
        /// </summary>
        public StateList<string> Tags
        {
            get
            {
                return this.tags;
            }
        }

        /// <summary>
        ///     Gets or sets the Title or the post.
        /// </summary>
        [Required(ErrorMessage = "Title is required")]
        [StringLength(50, ErrorMessage = "Title must be under 50 characters")]
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                if (this.title != value)
                {
                    this.MarkChanged("Title");
                }

                this.title = value;
            }
        }

        /// <summary>
        ///     Gets the trackback link to the post.
        /// </summary>
        public Uri TrackbackLink
        {
            get
            {
                return new Uri(string.Format("{0}trackback.axd?id={1}", Utils.AbsoluteWebRoot, this.Id));
            }
        }

        /// <summary>
        ///     Gets a value indicating whether or not the post is visible or not.
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.Authenticated ||
                       (this.Published && this.DateCreated <= DateTime.Now.AddHours(BlogSettings.Instance.Timezone));
            }
        }

        /// <summary>
        ///     Gets a value indicating whether a post is available to visitors not logged into the blog.
        /// </summary>
        public bool VisibleToPublic
        {
            get
            {
                return this.Published && this.DateCreated <= DateTime.Now.AddHours(BlogSettings.Instance.Timezone);
            }
        }

        public bool IsPhotoPost {
            get {
                if (this.PostType == Models.PostType.Photo) {
                    return true;
                }
                return false;
            }
        }


        #endregion

        #region Comment Properties

        /// <summary>
        ///     Gets a Collection of All Comments for the post
        /// </summary>
        public List<Comment> Comments
        {
            get
            {
                return this.comments;
            }
        }

        /// <summary>
        ///     Gets a Collection of All Comments for the post
        /// </summary>
        public List<Comment> AllComments
        {
            get
            {
                return this.comments.FindAll(c => !c.IsDeleted);
            }
        }

        /// <summary>
        ///     Gets a collection of Approved comments for the post sorted by date.
        ///     When moderation is enabled, unapproved comments go to pending.
        ///     Whith moderation off, they shown as approved.
        /// </summary>
        public List<Comment> ApprovedComments
        {
            get
            {
                if (BlogSettings.Instance.EnableCommentsModeration)
                {
                    return this.comments.FindAll(c => c.IsApproved && !c.IsSpam && !c.IsDeleted && !c.IsPingbackOrTrackback);
                }
                else
                {
                    return this.comments.FindAll(c => !c.IsSpam && !c.IsDeleted && !c.IsPingbackOrTrackback);
                }
            }
        }

        /// <summary>
        ///     Gets a collection of comments waiting for approval for the post, sorted by date
        ///     excluding comments rejected as spam
        /// </summary>
        public List<Comment> NotApprovedComments
        {
            get
            {
                return this.comments.FindAll(c => !c.IsApproved && !c.IsSpam && !c.IsDeleted && !c.IsPingbackOrTrackback);
            }
        }

        /// <summary>
        ///     Gets a collection of pingbacks and trackbacks for the post, sorted by date
        /// </summary>
        public List<Comment> Pingbacks
        {
            get
            {
                return this.comments.FindAll(c => c.IsApproved && !c.IsSpam && !c.IsDeleted && c.IsPingbackOrTrackback);
            }
        }

        /// <summary>
        ///     Gets a collection of comments marked as spam for the post, sorted by date.
        /// </summary>
        public List<Comment> SpamComments
        {
            get
            {
                return this.comments.FindAll(c => c.IsSpam && !c.IsDeleted);
            }
        }

        /// <summary>
        ///     Gets a collection of comments marked as deleted for the post, sorted by date.
        /// </summary>
        public List<Comment> DeletedComments
        {
            get
            {
                return this.comments.FindAll(c => c.IsDeleted);
            }
        }

        /// <summary>
        ///     Gets a collection of the comments that are nested as replies
        /// </summary>
        public List<Comment> NestedComments
        {
            get
            {
                if (this.nestedComments == null)
                {
                    this.CreateNestedComments();
                }

                return this.nestedComments;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has comments enabled.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has comments enabled; otherwise, <c>false</c>.
        /// </value>
        public bool HasCommentsEnabled
        {
            get
            {
                return this.hasCommentsEnabled;
            }

            set
            {
                if (this.hasCommentsEnabled != value)
                {
                    this.MarkChanged("hasCommentsEnabled");
                }

                this.hasCommentsEnabled = value;
            }
        }

        public bool ShowFacebookWidgets {
            get {
                return this.showFacebookWidgets;
            }

            set {
                if (this.showFacebookWidgets != value) {
                    this.MarkChanged("showFacebookWidgets");
                }

                this.showFacebookWidgets = value;
            }
        }

        #endregion

        #region Post Public Methods

        /// <summary>
        /// Returs a post based on the specified id.
        /// </summary>
        /// <param name="id">
        /// The post id.
        /// </param>
        /// <returns>
        /// The selected post.
        /// </returns>
        public static Post GetPost(Guid id)
        {
            return All.Find(p => p.Id == id);
        }

        //public static Post GetPhotoPost(Guid id) {
        //    return AllPhotoPosts.Find(p => p.Id == id);
        //}

        ///// <summary>
        ///// Returns a post based on it's title.
        ///// </summary>
        // public static Post GetPostBySlug(string slug, DateTime date)
        // {
        // return Posts.Find(delegate(Post p)
        // {
        // if (date != DateTime.MinValue && (p.DateCreated.Year != date.Year || p.DateCreated.Month != date.Month))
        // {
        // if (p.DateCreated.Day != 1 && p.DateCreated.Day != date.Day)
        // return false;
        // }

        // return slug.Equals(Utils.RemoveIllegalCharacters(p.Slug), StringComparison.OrdinalIgnoreCase);
        // });
        // }

        /// <summary>
        /// Returns all posts written by the specified author.
        /// </summary>
        /// <param name="author">
        /// The author.
        /// </param>
        /// <returns>
        /// A list of Post.
        /// </returns>
        public static List<Post> GetPostsByAuthor(string author)
        {
            var legalAuthor = Utils.RemoveIllegalCharacters(author);
            var list = All.FindAll(
                p =>
                {
                    var legalTitle = Utils.RemoveIllegalCharacters(p.Author);
                    return legalAuthor.Equals(legalTitle, StringComparison.OrdinalIgnoreCase);
                });

            list.TrimExcess();
            return list;
        }

        /// <summary>
        /// Returns all posts in the specified category
        /// </summary>
        /// <param name="categoryId">
        /// The category Id.
        /// </param>
        /// <returns>
        /// A list of Post.
        /// </returns>
        public static List<Post> GetPostsByCategory(Guid categoryId)
        {
            var cat = Category.GetCategory(categoryId);
            var col = All.FindAll(p => p.Categories.Contains(cat));
            col.Sort();
            col.TrimExcess();
            return col;
        }


        public static List<Post> GetPostsByKeyword(Guid keywordId) {
            var cat = Tag.GetTag(keywordId);
            var col = All.FindAll(p => p.Keywords.Contains(cat));
            col.Sort();
            col.TrimExcess();
            return col;
        }

        /// <summary>
        /// Returns all posts published between the two dates.
        /// </summary>
        /// <param name="dateFrom">
        /// The date From.
        /// </param>
        /// <param name="dateTo">
        /// The date To.
        /// </param>
        /// <returns>
        /// A list of Post.
        /// </returns>
        public static List<Post> GetPostsByDate(DateTime dateFrom, DateTime dateTo)
        {
            var list = All.FindAll(p => p.DateCreated.Date >= dateFrom && p.DateCreated.Date <= dateTo);
            list.TrimExcess();
            return list;
        }

        /// <summary>
        /// Returns all posts tagged with the specified tag.
        /// </summary>
        /// <param name="tag">
        /// The tag of the post.
        /// </param>
        /// <returns>
        /// A list of Post.
        /// </returns>
        public static List<Post> GetPostsByTag(string tag)
        {
            tag = Utils.RemoveIllegalCharacters(tag);
            var list =
                All.FindAll(
                    p =>
                    p.Tags.Any(t => Utils.RemoveIllegalCharacters(t).Equals(tag, StringComparison.OrdinalIgnoreCase)));

            list.TrimExcess();
            return list;
        }

        /// <summary>
        /// Checks to see if the specified title has already been used
        ///     by another post.
        ///     <remarks>
        /// Titles must be unique because the title is part of the URL.
        ///     </remarks>
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <returns>
        /// The is title unique.
        /// </returns>
        public static bool IsTitleUnique(string title)
        {
            var legal = Utils.RemoveIllegalCharacters(title);
            return
                All.All(
                    post => !Utils.RemoveIllegalCharacters(post.Title).Equals(legal, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Called when [serving].
        /// </summary>
        /// <param name="post">
        /// The post being served.
        /// </param>
        /// <param name="arg">
        /// The <see cref="BlogEngine.Core.ServingEventArgs"/> instance containing the event data.
        /// </param>
        public static void OnServing(Post post, ServingEventArgs arg)
        {
            if (Serving != null)
            {
                Serving(post, arg);
            }
        }

        /// <summary>
        /// Force reload of all posts
        /// </summary>
        //public static void Reload()
        //{
        //    posts = BlogProvider.Provider.FillPosts();
        //    posts.Sort();
        //}

        /// <summary>
        /// Imports Post (without all standard saving routines
        /// </summary>
        public void Import()
        {
            if (this.Deleted)
            {
                if (!this.New)
                {
                    BlogProvider.Provider.DeletePost(this);
                }
            }
            else
            {
                if (this.New)
                {
                    BlogProvider.Provider.InsertPost(this);
                }
                else
                {
                    BlogProvider.Provider.UpdatePost(this);
                }
            }
        }

        /// <summary>
        /// Marks the object as being an clean,
        ///     which means not dirty.
        /// </summary>
        public override void MarkOld()
        {
            this.Categories.MarkOld();
            this.Tags.MarkOld();
            this.NotificationEmails.MarkOld();
            base.MarkOld();
        }

        /// <summary>
        /// Adds a rating to the post.
        /// </summary>
        /// <param name="newRating">
        /// The rating.
        /// </param>
        public void Rate(int newRating)
        {
            if (this.Raters > 0)
            {
                var total = this.Raters * this.Rating;
                total += newRating;
                this.Raters++;
                this.Rating = total / this.Raters;
            }
            else
            {
                this.Raters = 1;
                this.Rating = newRating;
            }

            this.DataUpdate();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        #endregion

        #region Comment Public Methods

        /// <summary>
        /// Adds a comment to the collection and saves the post.
        /// </summary>
        /// <param name="comment">
        /// The comment to add to the post.
        /// </param>
        public void AddComment(Comment comment)
        {
            var e = new CancelEventArgs();
            
            this.Comments.Add(comment);
            this.DataUpdate();
            
            if (comment.IsApproved)
            {
                this.SendNotifications(comment);
            }
        }

        /// <summary>
        /// Approves all the comments in a post.  Included to save time on the approval process.
        /// </summary>
        public void ApproveAllComments()
        {
            foreach (var comment in this.Comments)
            {
                this.ApproveComment(comment);
            }
        }

        /// <summary>
        /// Approves a Comment for publication.
        /// </summary>
        /// <param name="comment">
        /// The Comment to approve
        /// </param>
        public void ApproveComment(Comment comment)
        {
            var e = new CancelEventArgs();
            var inx = this.Comments.IndexOf(comment);
            this.Comments[inx].IsApproved = true;
            this.Comments[inx].IsSpam = false;
            this.DateModified = comment.DateCreated;
            this.DataUpdate();
            this.SendNotifications(comment);
        }

        /// <summary>
        /// Disapproves a Comment as Spam.
        /// </summary>
        /// <param name="comment">
        /// The Comment to approve
        /// </param>
        public void DisapproveComment(Comment comment)
        {
            var e = new CancelEventArgs();
            var inx = this.Comments.IndexOf(comment);
            this.Comments[inx].IsApproved = false;
            this.Comments[inx].IsSpam = true;
            this.DateModified = comment.DateCreated;
            this.DataUpdate();
            this.SendNotifications(comment);
        }

        /// <summary>
        /// Imports a comment to comment collection and saves.  Does not
        ///     notify user or run extension events.
        /// </summary>
        /// <param name="comment">
        /// The comment to add to the post.
        /// </param>
        public void ImportComment(Comment comment)
        {
            this.Comments.Add(comment);
            this.DataUpdate();
        }

        /// <summary>
        /// Updates a comment in the collection and saves the post.
        /// </summary>
        /// <param name="comment">
        /// The comment to update in the post.
        /// </param>
        public void UpdateComment(Comment comment)
        {
            var e = new CancelEventArgs();

            var inx = this.Comments.IndexOf(comment);

            this.Comments[inx].IsApproved = comment.IsApproved;
            this.Comments[inx].Content = comment.Content;
            this.Comments[inx].Author = comment.Author;
            this.Comments[inx].Country = comment.Country;
            this.Comments[inx].Email = comment.Email;
            this.Comments[inx].IP = comment.IP;
            this.Comments[inx].Website = comment.Website;
            this.Comments[inx].ModeratedBy = comment.ModeratedBy;
            this.Comments[inx].IsSpam = comment.IsSpam;
            this.Comments[inx].IsDeleted = comment.IsDeleted;

            this.DataUpdate();
        }

        /// <summary>
        /// Removes a comment from the collection and saves the post.
        /// </summary>
        /// <param name="comment">
        /// The comment to remove from the post.
        /// </param>
        public void RemoveComment(Comment comment)
        {
            var e = new CancelEventArgs();
            this.Comments.Remove(comment);
            this.DataUpdate();
            
        }

        #endregion

        #region Implemented Interfaces

        #region IComparable<Post>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the 
        ///     objects being compared. The return value has the following meanings: 
        ///     Value Meaning Less than zero This object is less than the other parameter.Zero 
        ///     This object is equal to other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(Post other)
        {
            return other.DateCreated.CompareTo(this.DateCreated);
        }

        #endregion

        #region IPublishable

        /// <summary>
        /// Raises the Serving event
        /// </summary>
        /// <param name="eventArgs">
        /// The event Args.
        /// </param>
        public void OnServing(ServingEventArgs eventArgs)
        {
            if (Serving != null)
            {
                Serving(this, eventArgs);
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Deletes the Post from the current BlogProvider.
        /// </summary>
        public void Delete()
        {
            BlogProvider.Provider.DeletePost(this);
            if (!All.Contains(this))
            {
                return;
            }

            All.Remove(this);
        }

        /// <summary>
        /// Inserts a new post to the current BlogProvider.
        /// </summary>
        public void Save() {
            if (this.New) {
                if (this.DateCreated == DateTime.MinValue) {
                    this.DateCreated = DateTime.Now;
                }

                this.DateModified = DateTime.Now;
                // this.DataInsert();
                // Code for DataInsert
                BlogProvider.Provider.InsertPost(this);

                if (!this.New) {
                    return;
                }
                if (this.IsPhotoPost) {
                    PhotoPosts.Add(this);
                    PhotoPosts.Sort();
                } else {
                    Blogs.Add(this);
                    Blogs.Sort();
                }
                
            } else // This will be an Update
                {
                this.DateModified = DateTime.Now;

                this.DataUpdate();
                // Code for DataUpdate
                //BlogService.UpdatePost(this);
                //Posts.Sort();
                //AddRelations();
                //this.ResetNestedComments();
            }

            this.MarkOld();
        }

        /// <summary>
        /// Returns a Post based on the specified id.
        /// </summary>
        /// <param name="id">
        /// The post id.
        /// </param>
        /// <returns>
        /// The selected Post.
        /// </returns>
        protected Post DataSelect(Guid id)
        {
            return BlogProvider.Provider.SelectPost(id);
        }

        /// <summary>
        /// Updates the Post.
        /// </summary>
        protected void DataUpdate()
        {
            BlogProvider.Provider.UpdatePost(this);
            All.Sort();
            this.ResetNestedComments();
        }

        /// <summary>
        /// Sets the Previous and Next properties to all posts.
        /// </summary>
        //private static void AddRelations()
        //{
        //    for (var i = 0; i < posts.Count; i++)
        //    {
        //        posts[i].Next = null;
        //        posts[i].Previous = null;
        //        if (i > 0)
        //        {
        //            posts[i].Next = posts[i - 1];
        //        }

        //        if (i < posts.Count - 1)
        //        {
        //            posts[i].Previous = posts[i + 1];
        //        }
        //    }
        //}

        /// <summary>
        /// Nests comments based on Id and ParentId
        /// </summary>
        private void CreateNestedComments()
        {
            // instantiate object
            this.nestedComments = new List<Comment>();

            // temporary ID/Comment table
            var commentTable = new Hashtable();

            foreach (var comment in this.comments)
            {
                // add to hashtable for lookup
                commentTable.Add(comment.Id, comment);

                // check if this is a child comment
                if (comment.ParentId == Guid.Empty)
                {
                    // root comment, so add it to the list
                    this.nestedComments.Add(comment);
                }
                else
                {
                    // child comment, so find parent
                    var parentComment = commentTable[comment.ParentId] as Comment;
                    if (parentComment != null)
                    {
                        // double check that this sub comment has not already been added
                        if (parentComment.Comments.IndexOf(comment) == -1)
                        {
                            parentComment.Comments.Add(comment);
                        }
                    }
                    else
                    {
                        // just add to the base to prevent an error
                        this.nestedComments.Add(comment);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all nesting of comments
        /// </summary>
        private void ResetNestedComments()
        {
            // void the List<>
            this.nestedComments = null;

            // go through all comments and remove sub comments
            foreach (var c in this.Comments)
            {
                c.Comments.Clear();
            }
        }

        /// <summary>
        /// Sends a notification to all visitors  that has registered
        ///     to retrieve notifications for the specific post.
        /// </summary>
        /// <param name="comment">
        /// The comment.
        /// </param>
        private void SendNotifications(Comment comment)
        {
            if (this.NotificationEmails.Count == 0 || comment.IsApproved == false)
            {
                return;
            }

            foreach (var email in this.NotificationEmails)
            {
                if (email == comment.Email)
                {
                    continue;
                }

                // Intentionally using AbsoluteLink instead of PermaLink so the "unsubscribe-email" QS parameter
                // isn't dropped when post.aspx.cs does a 301 redirect to the RelativeLink, before the unsubscription
                // process takes place.
                var unsubscribeLink = this.AbsoluteLink.ToString();
                unsubscribeLink += string.Format(
                    "{0}unsubscribe-email={1}",
                    unsubscribeLink.Contains("?") ? "&" : "?",
                    HttpUtility.UrlEncode(email));

                var defaultCulture = Utils.GetDefaultCulture();

                var sb = new StringBuilder();
                sb.AppendFormat(
                    "<div style=\"font: 11px verdana, arial\">New Comment added by {0}<br /><br />", comment.Author);
                sb.AppendFormat("{0}<br /><br />", comment.Content.Replace(Environment.NewLine, "<br />"));
                sb.AppendFormat(
                    "<strong>{0}</strong>: <a href=\"{1}#id_{2}\">{3}</a><br/>",
                    Utils.Translate("post", null, defaultCulture),
                    this.PermaLink,
                    comment.Id,
                    this.Title);
                sb.Append("<br />_______________________________________________________________________________<br />");
                sb.AppendFormat(
                    "<a href=\"{0}\">{1}</a></div>", unsubscribeLink, Utils.Translate("commentNotificationUnsubscribe"));

                var mail = new MailMessage
                    {
                        From = new MailAddress(BlogSettings.Instance.Email, BlogSettings.Instance.Name),
                        Subject = string.Format("New comment on {0}", this.Title),
                        Body = sb.ToString()
                    };

                mail.To.Add(email);
                Utils.SendMailMessageAsync(mail);
            }
        }

        #endregion
    }

    public enum PostType{
        Blog = 1,
        Photo = 2,
        Video = 3,
        Audio
    }
}