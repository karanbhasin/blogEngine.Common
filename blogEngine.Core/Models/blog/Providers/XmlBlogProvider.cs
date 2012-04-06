using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Collections.Specialized;
using System.Text;
using System.Web.Configuration;
using System.Xml.Serialization;
using blogEngine.Core.Models;

namespace blogEngine.Core.Providers
{
    /// <summary>
    /// A storage provider for BlogEngine that uses XML files.
    ///     <remarks>
    /// To build another provider, you can just copy and modify
    ///         this one. Then add it to the web.config's BlogEngine section.
    ///     </remarks>
    /// </summary>
    public partial class XmlBlogProvider : BlogProvider
    {
        #region Posts
        // private static string _Folder = System.Web.HttpContext.Current.Server.MapPath(BlogSettings.Instance.StorageLocation);
        #region Properties

        /// <summary>
        ///     Gets _Folder.
        /// </summary>
        internal string Folder
        {
            get
            {
                var p = this.StorageLocation().Replace("~/", string.Empty);
                return Path.Combine(HttpRuntime.AppDomainAppPath, p);
            }
        }

        /// <summary>
        /// the folder for posts(non photo posts)
        /// </summary>
        internal const string PostsFolder = "posts";
        /// <summary>
        /// The folder for photo posts
        /// </summary>
        internal const string PhotoBlogFolder = "photoBlogs";
        /// <summary>
        /// the folder tofetch the posts from. methods like FillPosts need location to read the posts from.
        /// Instead of making a new method specifically for photoblogs, we will be changing the location of 
        /// the folder to fetch from in the fillPosts method call.
        /// </summary>
        internal string FetchPostsFolder = "posts";

        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object postSyncRoot = new object();

        private static List<Post> allPosts;
        public override List<Post> AllPosts {
            get {
                LoadPosts(false);
                LoadPhotoPosts(false);
                return posts.Concat(photoposts).ToList();
            }
        }

        /// <summary>
        /// The blog rolls.
        /// </summary>
        private static List<Post> posts;

        public override List<Post> Posts
        {
            get
            {
                LoadPosts();
                return posts;
            }
        }

        private void LoadPosts() {
            LoadPosts(false);
        }

        private void LoadPosts(bool reload) {
            if (posts == null || reload)//|| blogRolls.Count == 0)
                {
                lock (postSyncRoot) {
                    //if (posts == null || posts.Count == 0) {
                        posts = this.FillPosts();
                        posts.Sort();
                    //}
                }
            }
        }

        public override void Reload(int what) {
            if (what == 1) {
                LoadPosts(true);
            }

            if (what == 2) {
                LoadPhotoPosts(true);
            }
        }

        private static List<Post> photoposts;

        public override List<Post> PhotoPosts {
            get {
                LoadPhotoPosts();
                return photoposts;
            }
        }

        private void LoadPhotoPosts() {
            LoadPhotoPosts(false);
        }

        private void LoadPhotoPosts(bool reload) {
            if (photoposts == null || reload)//|| blogRolls.Count == 0)
                {
                lock (postSyncRoot) {
                    this.FetchPostsFolder = PhotoBlogFolder;
                    photoposts = this.FillPosts();
                    foreach (Post post in photoposts) {
                        post.PostType = PostType.Photo;
                    }
                    this.FetchPostsFolder = PostsFolder;
                    photoposts.Sort();
                }
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Deletes a post from the data store.
        /// </summary>
        /// <param name="post">
        /// The post to delete.
        /// </param>
        public override void DeletePost(Post post)
        {
            string fetchPostsFolder = PostsFolder;
            if (post.IsPhotoPost) {
                fetchPostsFolder = PhotoBlogFolder;
            }

            var fileName = string.Format("{0}{1}{2}{3}.xml", this.Folder, fetchPostsFolder, Path.DirectorySeparatorChar, post.Id);
            if (File.Exists(fileName))
            {
                // File.Delete(fileName);
                // Just rename the file so that it has an extension of .del at the end
                File.Move(fileName, fileName + ".del");
            }
        }

        /// <summary>
        /// Retrieves all posts from the data store
        /// </summary>
        /// <returns>
        /// List of Posts
        /// </returns>
        public override List<Post> FillPosts()
        {
            var folder = Category.Folder + FetchPostsFolder + Path.DirectorySeparatorChar;
            var p = (from file in Directory.GetFiles(folder, "*.xml", SearchOption.TopDirectoryOnly)
                         select new FileInfo(file)
                             into info
                             select info.Name.Replace(".xml", string.Empty)
                                 into id
                                 select SelectPost(new Guid(id))).ToList();

            p.Sort();
            return p;
        }


        public void WritePost(Post post, XmlWriter writer, bool topLevel = false) {

            writer.WriteStartElement("post");
            if (topLevel) {
                writer.WriteElementString("Id", post.Id.ToString());
                writer.WriteElementString("entityId", post.EntityID.ToString());
                writer.WriteElementString("postType", ((int)post.PostType).ToString());
                writer.WriteElementString(
               "pubDate",
               post.DateCreated.AddHours(-BlogSettings.Instance.Timezone).ToString(
                   "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                writer.WriteElementString(
                    "lastModified",
                    post.DateModified.AddHours(-BlogSettings.Instance.Timezone).ToString(
                        "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                writer.WriteElementString("publishDate", post.PublishDate.ToString());
            }
            writer.WriteElementString("author", post.Author);
            writer.WriteElementString("title", post.Title);
            writer.WriteElementString("description", post.Description);
            writer.WriteElementString("content", post.Content);
            writer.WriteElementString("contentURL", post.ContentURL);

            writer.WriteElementString("ispublished", post.Published.ToString());
            writer.WriteElementString("iscommentsenabled", post.HasCommentsEnabled.ToString());
           
            writer.WriteElementString("raters", post.Raters.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("rating", post.Rating.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("slug", post.Slug);

            // Tags
            writer.WriteStartElement("tags");
            foreach (var tag in post.Tags) {
                writer.WriteElementString("tag", tag);
            }

            writer.WriteEndElement();

            // comments
            #region Comments
            writer.WriteStartElement("comments");
            foreach (var comment in post.Comments) {
                writer.WriteStartElement("comment");
                writer.WriteAttributeString("id", comment.Id.ToString());
                writer.WriteAttributeString("parentid", comment.ParentId.ToString());
                writer.WriteAttributeString("approved", comment.IsApproved.ToString());
                writer.WriteAttributeString("spam", comment.IsSpam.ToString());
                writer.WriteAttributeString("deleted", comment.IsDeleted.ToString());

                writer.WriteElementString(
                    "date",
                    comment.DateCreated.AddHours(-BlogSettings.Instance.Timezone).ToString(
                        "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                writer.WriteElementString("author", comment.Author);
                writer.WriteElementString("email", comment.Email);
                writer.WriteElementString("country", comment.Country);
                writer.WriteElementString("ip", comment.IP);

                if (comment.Website != null) {
                    writer.WriteElementString("website", comment.Website.ToString());
                }

                if (!string.IsNullOrEmpty(comment.ModeratedBy)) {
                    writer.WriteElementString("moderatedby", comment.ModeratedBy);
                }

                if (comment.Avatar != null) {
                    writer.WriteElementString("avatar", comment.Avatar);
                }

                writer.WriteElementString("content", comment.Content);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            #endregion

            // categories
            writer.WriteStartElement("categories");
            foreach (var cat in post.Categories) {
                writer.WriteElementString("category", cat.Id.ToString());
            }

            writer.WriteEndElement();

            // keywords
            writer.WriteStartElement("keywords");
            foreach (var keyword in post.Keywords) {
                writer.WriteElementString("keyword", keyword.Id.ToString());
            }

            writer.WriteEndElement();

            // Notification e-mails
            writer.WriteStartElement("notifications");
            foreach (var email in post.NotificationEmails) {
                writer.WriteElementString("email", email);
            }

            writer.WriteEndElement();

            //Nested posts
            writer.WriteStartElement("posts");
            foreach (var nestedPost in post.Posts) {
                WritePost(nestedPost, writer);
            }
            writer.WriteEndElement();

            // for </post>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Inserts a new Post to the data store.
        /// </summary>
        /// <param name="post">
        /// The post to insert.
        /// </param>
        public override void InsertPost(Post post) {
            string fetchPostsFolder = PostsFolder;
            if (post.IsPhotoPost) {
                fetchPostsFolder = PhotoBlogFolder;
            }

            if (!Directory.Exists(string.Format("{0}{1}", this.Folder, fetchPostsFolder))) {
                Directory.CreateDirectory(string.Format("{0}{1}", this.Folder, fetchPostsFolder));
            }

            var fileName = string.Format("{0}{1}{2}{3}.xml", this.Folder, fetchPostsFolder, Path.DirectorySeparatorChar, post.Id);
            var settings = new XmlWriterSettings { Indent = true };

            var ms = new MemoryStream();

            using (var writer = XmlWriter.Create(ms, settings)) {
                writer.WriteStartDocument(true);
                WritePost(post, writer, true);
                writer.WriteEndDocument();
            }

            using (var fs = File.Open(fileName, FileMode.Create, FileAccess.Write)) {
                ms.WriteTo(fs);
                ms.Dispose();
            }
        }


        /// <summary>
        /// Retrieves a Post from the provider based on the specified id.
        /// </summary>
        /// <param name="id">
        /// The Post id.
        /// </param>
        /// <returns>
        /// A Post object.
        /// </returns>
        public override Post SelectPost(Guid id) {
            var fileName = string.Format("{0}{1}{2}{3}.xml", this.Folder, FetchPostsFolder, Path.DirectorySeparatorChar, id);
            //var post = new Post();
            //post.Id = id;
            //post.MarkOld();
            var doc = new XmlDocument();
            doc.Load(fileName);
            var post = ReadPost(doc.SelectSingleNode("post"), true);
            post.MarkOld();
            return post;
        }
        /// <summary>
        /// Retrieves a Post from the provider based on the specified id.
        /// </summary>
        /// <param name="id">
        /// The Post id.
        /// </param>
        /// <returns>
        /// A Post object.
        /// </returns>
        public Post ReadPost(XmlNode doc, bool topLevel = false)
        {
        //    public static T Load(TKey id)
        //{
        //    var instance = new T();
        //    instance = instance.DataSelect(id); // SelectPost is called here
        //    instance.Id = id;

        //    instance.MarkOld();
        //    return instance;
        //}

           

            //post.Title = doc.SelectSingleNode("post/title").InnerText;
            //post.Description = doc.SelectSingleNode("post/description").InnerText;
            //post.Content = doc.SelectSingleNode("post/content").InnerText;

            //if (doc.SelectSingleNode("post/entityId") != null) {
            //    post.EntityID = Int32.Parse(doc.SelectSingleNode("post/entityId").InnerText);
            //}

            //if (doc.SelectSingleNode("post/pubDate") != null)
            //{
            //    post.DateCreated = DateTime.Parse(
            //        doc.SelectSingleNode("post/pubDate").InnerText, CultureInfo.InvariantCulture);
            //}

            //if (doc.SelectSingleNode("post/lastModified") != null)
            //{
            //    post.DateModified = DateTime.Parse(
            //        doc.SelectSingleNode("post/lastModified").InnerText, CultureInfo.InvariantCulture);
            //}

            //if (doc.SelectSingleNode("post/author") != null)
            //{
            //    post.Author = doc.SelectSingleNode("post/author").InnerText;
            //}

            //if (doc.SelectSingleNode("post/ispublished") != null)
            //{
            //    post.Published = bool.Parse(doc.SelectSingleNode("post/ispublished").InnerText);
            //}

            //if (doc.SelectSingleNode("post/iscommentsenabled") != null)
            //{
            //    post.HasCommentsEnabled = bool.Parse(doc.SelectSingleNode("post/iscommentsenabled").InnerText);
            //}

            //if (doc.SelectSingleNode("post/raters") != null)
            //{
            //    post.Raters = int.Parse(doc.SelectSingleNode("post/raters").InnerText, CultureInfo.InvariantCulture);
            //}

            //if (doc.SelectSingleNode("post/rating") != null)
            //{
            //    post.Rating = float.Parse(
            //        doc.SelectSingleNode("post/rating").InnerText, CultureInfo.GetCultureInfo("en-gb"));
            //}

            //if (doc.SelectSingleNode("post/slug") != null)
            //{
            //    post.Slug = doc.SelectSingleNode("post/slug").InnerText;
            //}

            //// Tags
            //foreach (var node in
            //    doc.SelectNodes("post/tags/tag").Cast<XmlNode>().Where(node => !string.IsNullOrEmpty(node.InnerText)))
            //{
            //    post.Tags.Add(node.InnerText);
            //}

            //// comments
            //foreach (XmlNode node in doc.SelectNodes("post/comments/comment"))
            //{
            //    var comment = new Comment
            //    {
            //        Id = new Guid(node.Attributes["id"].InnerText),
            //        ParentId =
            //            (node.Attributes["parentid"] != null)
            //                ? new Guid(node.Attributes["parentid"].InnerText)
            //                : Guid.Empty,
            //        Author = node.SelectSingleNode("author").InnerText,
            //        Email = node.SelectSingleNode("email").InnerText,
            //        Parent = post
            //    };

            //    if (node.SelectSingleNode("country") != null)
            //    {
            //        comment.Country = node.SelectSingleNode("country").InnerText;
            //    }

            //    if (node.SelectSingleNode("ip") != null)
            //    {
            //        comment.IP = node.SelectSingleNode("ip").InnerText;
            //    }

            //    if (node.SelectSingleNode("website") != null)
            //    {
            //        Uri website;
            //        if (Uri.TryCreate(node.SelectSingleNode("website").InnerText, UriKind.Absolute, out website))
            //        {
            //            comment.Website = website;
            //        }
            //    }

            //    if (node.SelectSingleNode("moderatedby") != null)
            //    {
            //        comment.ModeratedBy = node.SelectSingleNode("moderatedby").InnerText;
            //    }

            //    comment.IsApproved = node.Attributes["approved"] == null ||
            //                         bool.Parse(node.Attributes["approved"].InnerText);

            //    if (node.SelectSingleNode("avatar") != null)
            //    {
            //        comment.Avatar = node.SelectSingleNode("avatar").InnerText;
            //    }

            //    comment.IsSpam = node.Attributes["spam"] == null ? false :
            //                        bool.Parse(node.Attributes["spam"].InnerText);

            //    comment.IsDeleted = node.Attributes["deleted"] == null ? false :
            //                        bool.Parse(node.Attributes["deleted"].InnerText);

            //    comment.Content = node.SelectSingleNode("content").InnerText;
            //    comment.DateCreated = DateTime.Parse(
            //        node.SelectSingleNode("date").InnerText, CultureInfo.InvariantCulture);

            //    post.Comments.Add(comment);
            //}

            //post.Comments.Sort();

            //// categories
            //foreach (var cat in from XmlNode node in doc.SelectNodes("post/categories/category")
            //                    select new Guid(node.InnerText)
            //                        into key
            //                        select Category.GetCategory(key)
            //                            into cat
            //                            where cat != null
            //                            select cat)
            //{
            //    // CategoryDictionary.Instance.ContainsKey(key))
            //    post.Categories.Add(cat);
            //}

            //// Keywords
            //// categories
            //foreach (var keyword in from XmlNode node in doc.SelectNodes("post/keywords/keyword")
            //                    select new Guid(node.InnerText)
            //                        into key
            //                        select Tag.GetTag(key)
            //                            into keyword
            //                            where keyword != null
            //                            select keyword) {
            //    post.Keywords.Add(keyword);
            //}

            //// Notification e-mails
            //foreach (XmlNode node in doc.SelectNodes("post/notifications/email"))
            //{
            //    post.NotificationEmails.Add(node.InnerText);
            //}

            //// Nested posts
            //// In case of a photo post, this will be a list of all the photos
            //foreach (XmlNode nestedPost in doc.SelectNodes("post/posts/post")) {
            //    Author = nestedPost.SelectSingleNode("author").InnerText,
            //}
            //return post;
             var post = new Post();
             if (doc.SelectSingleNode("Id") != null) {
                 post.Id = new Guid(doc.SelectSingleNode("Id").InnerText);
             }
            post.Title = doc.SelectSingleNode("title").InnerText;
            post.Description = doc.SelectSingleNode("description").InnerText;
            post.Content = doc.SelectSingleNode("content").InnerText;
            if (topLevel) {
                if (doc.SelectSingleNode("photoType") != null) {
                    post.PostType = (PostType)Convert.ToInt32(doc.SelectSingleNode("photoType").InnerText);
                    if (doc.SelectSingleNode("publishDate") != null) {
                        post.PublishDate = DateTime.Parse(
                            doc.SelectSingleNode("publishDate").InnerText, CultureInfo.InvariantCulture);
                    }
                }
            }

            if (doc.SelectSingleNode("contentURL") != null) {
                post.ContentURL = doc.SelectSingleNode("contentURL").InnerText;
            }

            if (doc.SelectSingleNode("entityId") != null) {
                post.EntityID = Int32.Parse(doc.SelectSingleNode("entityId").InnerText);
            }

            if (doc.SelectSingleNode("pubDate") != null)
            {
                post.DateCreated = DateTime.Parse(
                    doc.SelectSingleNode("pubDate").InnerText, CultureInfo.InvariantCulture);
            }

            if (doc.SelectSingleNode("lastModified") != null)
            {
                post.DateModified = DateTime.Parse(
                    doc.SelectSingleNode("lastModified").InnerText, CultureInfo.InvariantCulture);
            }

            if (doc.SelectSingleNode("author") != null)
            {
                post.Author = doc.SelectSingleNode("author").InnerText;
            }

            if (doc.SelectSingleNode("ispublished") != null)
            {
                post.Published = bool.Parse(doc.SelectSingleNode("ispublished").InnerText);
            }

            if (doc.SelectSingleNode("iscommentsenabled") != null)
            {
                post.HasCommentsEnabled = bool.Parse(doc.SelectSingleNode("iscommentsenabled").InnerText);
            }

            if (doc.SelectSingleNode("raters") != null)
            {
                post.Raters = int.Parse(doc.SelectSingleNode("raters").InnerText, CultureInfo.InvariantCulture);
            }

            if (doc.SelectSingleNode("rating") != null)
            {
                post.Rating = float.Parse(
                    doc.SelectSingleNode("rating").InnerText, CultureInfo.GetCultureInfo("en-gb"));
            }

            if (doc.SelectSingleNode("slug") != null)
            {
                post.Slug = doc.SelectSingleNode("slug").InnerText;
            }

            // Tags
            foreach (var node in
                doc.SelectNodes("tags/tag").Cast<XmlNode>().Where(node => !string.IsNullOrEmpty(node.InnerText)))
            {
                post.Tags.Add(node.InnerText);
            }

            // comments
            foreach (XmlNode node in doc.SelectNodes("comments/comment"))
            {
                var comment = new Comment
                {
                    Id = new Guid(node.Attributes["id"].InnerText),
                    ParentId =
                        (node.Attributes["parentid"] != null)
                            ? new Guid(node.Attributes["parentid"].InnerText)
                            : Guid.Empty,
                    Author = node.SelectSingleNode("author").InnerText,
                    Email = node.SelectSingleNode("email").InnerText,
                    Parent = post
                };

                if (node.SelectSingleNode("country") != null)
                {
                    comment.Country = node.SelectSingleNode("country").InnerText;
                }

                if (node.SelectSingleNode("ip") != null)
                {
                    comment.IP = node.SelectSingleNode("ip").InnerText;
                }

                if (node.SelectSingleNode("website") != null)
                {
                    Uri website;
                    if (Uri.TryCreate(node.SelectSingleNode("website").InnerText, UriKind.Absolute, out website))
                    {
                        comment.Website = website;
                    }
                }

                if (node.SelectSingleNode("moderatedby") != null)
                {
                    comment.ModeratedBy = node.SelectSingleNode("moderatedby").InnerText;
                }

                comment.IsApproved = node.Attributes["approved"] == null ||
                                     bool.Parse(node.Attributes["approved"].InnerText);

                if (node.SelectSingleNode("avatar") != null)
                {
                    comment.Avatar = node.SelectSingleNode("avatar").InnerText;
                }

                comment.IsSpam = node.Attributes["spam"] == null ? false :
                                    bool.Parse(node.Attributes["spam"].InnerText);

                comment.IsDeleted = node.Attributes["deleted"] == null ? false :
                                    bool.Parse(node.Attributes["deleted"].InnerText);

                comment.Content = node.SelectSingleNode("content").InnerText;
                comment.DateCreated = DateTime.Parse(
                    node.SelectSingleNode("date").InnerText, CultureInfo.InvariantCulture);

                post.Comments.Add(comment);
            }

            post.Comments.Sort();

            // categories
            foreach (var cat in from XmlNode node in doc.SelectNodes("categories/category")
                                select new Guid(node.InnerText)
                                    into key
                                    select Category.GetCategory(key)
                                        into cat
                                        where cat != null
                                        select cat)
            {
                // CategoryDictionary.Instance.ContainsKey(key))
                post.Categories.Add(cat);
            }

            // Keywords
            // categories
            foreach (var keyword in from XmlNode node in doc.SelectNodes("keywords/keyword")
                                select new Guid(node.InnerText)
                                    into key
                                    select Tag.GetTag(key)
                                        into keyword
                                        where keyword != null
                                        select keyword) {
                post.Keywords.Add(keyword);
            }

            // Notification e-mails
            foreach (XmlNode node in doc.SelectNodes("notifications/email"))
            {
                post.NotificationEmails.Add(node.InnerText);
            }

            // Nested posts
            // In case of a photo post, this will be a list of all the photos
            foreach (XmlNode nestedPost in doc.SelectNodes("posts/post")) {
                post.Posts.Add(ReadPost(nestedPost));
            }
            return post;
        }

        /// <summary>
        /// Updates an existing Post in the data store specified by the provider.
        /// </summary>
        /// <param name="post">
        /// The post to update.
        /// </param>
        public override void UpdatePost(Post post)
        {
            this.InsertPost(post);
        }

        #endregion
        #endregion

        #region Blog Roll
        #region Public Methods
        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object blogSyncRoot = new object();

        /// <summary>
        /// The blog rolls.
        /// </summary>
        private static List<BlogRollItem> blogRolls;

        public override List<BlogRollItem> BlogRolls
        {
            get
            {
                if (blogRolls == null || blogRolls.Count == 0)
                {
                    lock (blogSyncRoot)
                    {
                        if (blogRolls == null || blogRolls.Count == 0)
                        {
                            blogRolls = this.FillBlogRoll();
                            blogRolls.Sort();
                        }
                    }
                }
                return blogRolls;
            }
        }

        /// <summary>
        /// Deletes a BlogRoll
        /// </summary>
        /// <param name="blogRollItem">
        /// The blog Roll Item.
        /// </param>
        public override void DeleteBlogRollItem(BlogRollItem blogRollItem)
        {
            BlogRolls.Remove(blogRollItem);
            this.WriteBlogRollFile(blogRolls);
        }

        /// <summary>
        /// Fills an unsorted list of BlogRolls.
        /// </summary>
        /// <returns>
        /// A List&lt;BlogRoll&gt; of all BlogRolls
        /// </returns>
        public override List<BlogRollItem> FillBlogRoll()
        {
            var fileName = this.Folder + "blogroll.xml";
            if (!File.Exists(fileName))
            {
                return null;
            }

            var doc = new XmlDocument();
            doc.Load(fileName);
            var blogRoll = new List<BlogRollItem>();

            var largestSortIndex = -1;
            var legacyFormat = false;
            var nodes = doc.SelectNodes("blogRoll/item");
            if (nodes != null)
            {
                if (nodes.Count == 0)
                {
                    // legacy file format.
                    nodes = doc.SelectNodes("opml/body/outline");
                    legacyFormat = true;
                }

                foreach (var br in from XmlNode node in nodes
                                   select new BlogRollItem
                                   {
                                       Id = node.Attributes["id"] == null ? Guid.NewGuid() : new Guid(node.Attributes["id"].InnerText),
                                       Title = node.Attributes["title"] == null ? null : node.Attributes["title"].InnerText,
                                       Description = node.Attributes["description"] == null ? null : node.Attributes["description"].InnerText,
                                       BlogUrl = node.Attributes["htmlUrl"] == null ? null : new Uri(node.Attributes["htmlUrl"].InnerText),
                                       FeedUrl = node.Attributes["xmlUrl"] == null ? null : new Uri(node.Attributes["xmlUrl"].InnerText),
                                       Xfn = node.Attributes["xfn"] == null ? null : node.Attributes["xfn"].InnerText,
                                       SortIndex = node.Attributes["sortIndex"] == null ? (blogRoll.Count == 0 ? 0 : largestSortIndex + 1) : int.Parse(node.Attributes["sortIndex"].InnerText)
                                   })
                {
                    if (br.SortIndex > largestSortIndex)
                    {
                        largestSortIndex = br.SortIndex;
                    }

                    blogRoll.Add(br);
                    br.MarkOld();
                }
            }

            if (legacyFormat && blogRoll.Count > 0)
            {
                // if we're upgrading from a legacy format, re-write the file to conform to the new format.
                this.WriteBlogRollFile(blogRoll);
            }

            return blogRoll;
        }

        /// <summary>
        /// Inserts a BlogRoll
        /// </summary>
        /// <param name="blogRollItem">
        /// The blog Roll Item.
        /// </param>
        public override void InsertBlogRollItem(BlogRollItem blogRollItem)
        {
            var blogRolls = BlogRolls;
            blogRolls.Add(blogRollItem);

            this.WriteBlogRollFile(blogRolls);
        }

        /// <summary>
        /// Gets a BlogRoll based on a Guid.
        /// </summary>
        /// <param name="id">
        /// The BlogRoll's Guid.
        /// </param>
        /// <returns>
        /// A matching BlogRoll
        /// </returns>
        public override BlogRollItem SelectBlogRollItem(Guid id)
        {
            var blogRoll = BlogRolls.Find(br => br.Id == id) ?? new BlogRollItem();

            blogRoll.MarkOld();
            return blogRoll;
        }

        /// <summary>
        /// Updates a BlogRoll
        /// </summary>
        /// <param name="blogRollItem">
        /// The blog Roll Item.
        /// </param>
        public override void UpdateBlogRollItem(BlogRollItem blogRollItem)
        {
            var blogRolls = BlogRolls;
            blogRolls.Remove(blogRollItem);
            blogRolls.Add(blogRollItem);
            this.WriteBlogRollFile(blogRolls);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The write blog roll file.
        /// </summary>
        /// <param name="blogRollItems">
        /// The blog roll items.
        /// </param>
        private void WriteBlogRollFile(List<BlogRollItem> blogRollItems)
        {
            var fileName = this.Folder + "blogroll.xml";

            using (var writer = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.WriteStartDocument(true);
                writer.WriteStartElement("blogRoll");

                foreach (var br in blogRollItems)
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("id", br.Id.ToString());
                    writer.WriteAttributeString("title", br.Title);
                    writer.WriteAttributeString("description", br.Description ?? string.Empty);
                    writer.WriteAttributeString("htmlUrl", br.BlogUrl != null ? br.BlogUrl.ToString() : string.Empty);
                    writer.WriteAttributeString("xmlUrl", br.FeedUrl != null ? br.FeedUrl.ToString() : string.Empty);
                    writer.WriteAttributeString("xfn", br.Xfn ?? string.Empty);
                    writer.WriteAttributeString("sortIndex", br.SortIndex.ToString());
                    writer.WriteEndElement();
                    br.MarkOld();
                }

                writer.WriteEndElement();
            }
        }

        #endregion
        #endregion

        #region Categories
        #region Public Methods

        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object categorySyncRoot = new object();

        /// <summary>
        /// The blog rolls.
        /// </summary>
        private static List<Category> categories;

        /// <summary>
        ///     Gets an unsorted list of all Categories.
        /// </summary>
        /// <value>The categories.</value>
        public override List<Category> Categories
        {
            get
            {
                if (categories == null)
                {
                    lock (categorySyncRoot)
                    {
                        if (categories == null)
                        {
                            categories = FillCategories();
                            categories.Sort();
                        }
                    }
                }

                return categories;
            }
        }

        /// <summary>
        /// Deletes a Category
        /// </summary>
        /// <param name="category">
        /// Must be a valid Category object.
        /// </param>
        public override void DeleteCategory(Category category)
        {
            var categories = Categories;
            categories.Remove(category);

            if (Categories.Contains(category))
            {
                Categories.Remove(category);
            }

            this.WriteToFile();
        }

        /// <summary>
        /// Fills an unsorted list of categories.
        /// </summary>
        /// <returns>
        /// A List&lt;Category&gt; of all Categories.
        /// </returns>
        public override List<Category> FillCategories()
        {
            var fileName = this.Folder + "categories.xml";
            if (!File.Exists(fileName))
            {
                return null;
            }

            var doc = new XmlDocument();
            doc.Load(fileName);
            var categories = new List<Category>();

            foreach (XmlNode node in doc.SelectNodes("categories/category"))
            {
                var category = new Category { Id = new Guid(node.Attributes["id"].InnerText), Title = node.InnerText };

                if (node.Attributes["description"] != null)
                {
                    category.Description = node.Attributes["description"].InnerText;
                }
                else
                {
                    category.Description = string.Empty;
                }

                if (node.Attributes["parent"] != null)
                {
                    if (String.IsNullOrEmpty(node.Attributes["parent"].InnerText))
                    {
                        category.Parent = null;
                    }
                    else
                    {
                        category.Parent = new Guid(node.Attributes["parent"].InnerText);
                    }
                }
                else
                {
                    category.Parent = null;
                }

                categories.Add(category);
                category.MarkOld();
            }

            return categories;
        }

        /// <summary>
        /// Inserts a Category
        /// </summary>
        /// <param name="category">
        /// Must be a valid Category object.
        /// </param>
        public override void InsertCategory(Category category)
        {
            var categories = Categories;
            categories.Add(category);

            this.WriteToFile();
        }

        /// <summary>
        /// Gets a Category based on a Guid
        /// </summary>
        /// <param name="id">
        /// The category's Guid.
        /// </param>
        /// <returns>
        /// A matching Category
        /// </returns>
        public override Category SelectCategory(Guid id)
        {
            var categories = Categories;

            var category = categories.FirstOrDefault(cat => cat.Id == id) ?? new Category();
            category.MarkOld();
            return category;
        }

        /// <summary>
        /// Updates a Category
        /// </summary>
        /// <param name="category">
        /// Must be a valid Category object.
        /// </param>
        public override void UpdateCategory(Category category)
        {
            var categories = Categories;
            categories.Remove(category);
            categories.Add(category);

            this.WriteToFile();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Saves the Categories to disk.
        /// </summary>
        private void WriteToFile()
        {
            var categories = Categories;
            var fileName = string.Format("{0}categories.xml", this.Folder);

            using (var writer = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.WriteStartDocument(true);
                writer.WriteStartElement("categories");

                foreach (var cat in categories)
                {
                    writer.WriteStartElement("category");
                    writer.WriteAttributeString("id", cat.Id.ToString());
                    writer.WriteAttributeString("description", cat.Description);
                    writer.WriteAttributeString("parent", cat.Parent.ToString());
                    writer.WriteValue(cat.Title);
                    writer.WriteEndElement();
                    cat.MarkOld();
                }

                writer.WriteEndElement();
            }
        }

        #endregion
        #endregion


        #region Tags
        #region Public Methods

        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object tagSyncRoot = new object();

        /// <summary>
        /// The blog rolls.
        /// </summary>
        private static List<Tag> tags;

        /// <summary>
        ///     Gets an unsorted list of all Categories.
        /// </summary>
        /// <value>The categories.</value>
        public override List<Tag> Tags {
            get {
                if (tags == null) {
                    lock (tagSyncRoot) {
                        if (tags == null) {
                            tags = FillTags();
                            tags.Sort();
                        }
                    }
                }

                return tags;
            }
        }

        /// <summary>
        /// Deletes a Tag
        /// </summary>
        /// <param name="category">
        /// Must be a valid Tag object.
        /// </param>
        public override void DeleteTag(Tag tag) {
            var tags = Tags;
            tags.Remove(tag);

            if (Tags.Contains(tag)) {
                tags.Remove(tag);
            }

            this.WriteTagsToFile();
        }

        /// <summary>
        /// Fills an unsorted list of categories.
        /// </summary>
        /// <returns>
        /// A List&lt;Category&gt; of all Categories.
        /// </returns>
        public override List<Tag> FillTags() {
            var fileName = this.Folder + "tags.xml";
            if (!File.Exists(fileName)) {
                return null;
            }

            var doc = new XmlDocument();
            doc.Load(fileName);
            var tags = new List<Tag>();

            foreach (XmlNode node in doc.SelectNodes("tags/tag")) {
                var tag = new Tag { Id = new Guid(node.Attributes["id"].InnerText), Text = node.InnerText };
                tags.Add(tag);
                tag.MarkOld();
            }

            return tags;
        }

        /// <summary>
        /// Inserts a Category
        /// </summary>
        /// <param name="category">
        /// Must be a valid Category object.
        /// </param>
        public override void InsertTag(Tag tag) {
            var tags = Tags;
            tags.Add(tag);
            this.WriteTagsToFile();
        }
        /// <summary>
        /// Gets a Category based on a Guid
        /// </summary>
        /// <param name="id">
        /// The category's Guid.
        /// </param>
        /// <returns>
        /// A matching Category
        /// </returns>
        public override Tag SelectTag(Guid id) {
            var tags = Tags;

            var tag = tags.FirstOrDefault(t => t.Id == id) ?? new Tag();
            tag.MarkOld();
            return tag;
        }

        /// <summary>
        /// Updates a Category
        /// </summary>
        /// <param name="category">
        /// Must be a valid Category object.
        /// </param>
        public override void UpdateTag(Tag tag) {
            var tags = Tags;
            tags.Remove(tag);
            tags.Add(tag);

            this.WriteTagsToFile();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Saves the Categories to disk.
        /// </summary>
        private void WriteTagsToFile() {
            var tags = Tags;
            var fileName = string.Format("{0}tags.xml", this.Folder);

            using (var writer = new XmlTextWriter(fileName, Encoding.UTF8)) {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.WriteStartDocument(true);
                writer.WriteStartElement("tags");

                foreach (var t in tags) {
                    writer.WriteStartElement("tag");
                    writer.WriteAttributeString("id", t.Id.ToString());
                    writer.WriteValue(t.Text);
                    writer.WriteEndElement();
                    t.MarkOld();
                }

                writer.WriteEndElement();
            }
        }

        #endregion
        #endregion

        #region Pages
        #region Public Methods

        /// <summary>
        /// Deletes a Page from the data store specified by the provider.
        /// </summary>
        /// <param name="page">The page to delete.</param>
        public override void DeletePage(Page page)
        {
            var fileName = string.Format("{0}pages{1}{2}.xml", this.Folder, Path.DirectorySeparatorChar, page.Id);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            if (Page.All.Contains(page))
            {
                Page.All.Remove(page);
            }
        }

        /// <summary>
        /// Retrieves all pages from the data store
        /// </summary>
        /// <returns>
        /// List of Pages
        /// </returns>
        public override List<Page> FillPages()
        {
            var folder = string.Format("{0}pages{1}", Category.Folder, Path.DirectorySeparatorChar);

            return (from file in Directory.GetFiles(folder, "*.xml", SearchOption.TopDirectoryOnly)
                    select new FileInfo(file)
                        into info
                        select info.Name.Replace(".xml", string.Empty)
                            into id
                            select SelectPage(new Guid(id))).ToList();
        }

        /// <summary>
        /// Inserts a new Page into the data store specified by the provider.
        /// </summary>
        /// <param name="page">The page to insert.</param>
        public override void InsertPage(Page page)
        {
            if (!Directory.Exists(string.Format("{0}pages", this.Folder)))
            {
                Directory.CreateDirectory(string.Format("{0}pages", this.Folder));
            }

            var fileName = string.Format("{0}pages{1}{2}.xml", this.Folder, Path.DirectorySeparatorChar, page.Id);
            var settings = new XmlWriterSettings { Indent = true };

            using (var writer = XmlWriter.Create(fileName, settings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("page");

                writer.WriteElementString("title", page.Title);
                writer.WriteElementString("description", page.Description);
                writer.WriteElementString("content", page.Content);
                writer.WriteElementString("keywords", page.Keywords);
                writer.WriteElementString("slug", page.Slug);
                writer.WriteElementString("parent", page.Parent.ToString());
                writer.WriteElementString("isfrontpage", page.FrontPage.ToString());
                writer.WriteElementString("showinlist", page.ShowInList.ToString());
                writer.WriteElementString("ispublished", page.Published.ToString());
                writer.WriteElementString(
                    "datecreated",
                    page.DateCreated.AddHours(-BlogSettings.Instance.Timezone).ToString(
                        "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                writer.WriteElementString(
                    "datemodified",
                    page.DateModified.AddHours(-BlogSettings.Instance.Timezone).ToString(
                        "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));

                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Retrieves a Page from the provider based on the specified id.
        /// </summary>
        /// <param name="id">The Page id.</param>
        /// <returns>The Page object.</returns>
        public override Page SelectPage(Guid id)
        {
            var fileName = string.Format("{0}pages{1}{2}.xml", this.Folder, Path.DirectorySeparatorChar, id);
            var doc = new XmlDocument();
            doc.Load(fileName);

            var page = new Page
            {
                Title = doc.SelectSingleNode("page/title").InnerText,
                Description = doc.SelectSingleNode("page/description").InnerText,
                Content = doc.SelectSingleNode("page/content").InnerText,
                Keywords = doc.SelectSingleNode("page/keywords").InnerText
            };

            if (doc.SelectSingleNode("page/slug") != null)
            {
                page.Slug = doc.SelectSingleNode("page/slug").InnerText;
            }

            if (doc.SelectSingleNode("page/parent") != null)
            {
                page.Parent = new Guid(doc.SelectSingleNode("page/parent").InnerText);
            }

            if (doc.SelectSingleNode("page/isfrontpage") != null)
            {
                page.FrontPage = bool.Parse(doc.SelectSingleNode("page/isfrontpage").InnerText);
            }

            if (doc.SelectSingleNode("page/showinlist") != null)
            {
                page.ShowInList = bool.Parse(doc.SelectSingleNode("page/showinlist").InnerText);
            }

            if (doc.SelectSingleNode("page/ispublished") != null)
            {
                page.Published = bool.Parse(doc.SelectSingleNode("page/ispublished").InnerText);
            }

            page.DateCreated = DateTime.Parse(
                doc.SelectSingleNode("page/datecreated").InnerText, CultureInfo.InvariantCulture);
            page.DateModified = DateTime.Parse(
                doc.SelectSingleNode("page/datemodified").InnerText, CultureInfo.InvariantCulture);

            return page;
        }

        /// <summary>
        /// Updates an existing Page in the data store specified by the provider.
        /// </summary>
        /// <param name="page">The page to update.</param>
        public override void UpdatePage(Page page)
        {
            this.InsertPage(page);
        }


        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object pageSyncRoot = new object();

        /// <summary>
        /// The blog rolls.
        /// </summary>
        private static List<Page> pages;

        public override List<Page> Pages
        {
            get
            {
                if (pages == null || pages.Count == 0)
                {
                    lock (pageSyncRoot)
                    {
                        if (pages == null || pages.Count == 0)
                        {
                            pages = this.FillPages();
                            pages.Sort();
                        }
                    }
                }
                return pages;
            }
        }
        #endregion
        #endregion

        #region PingServices
        #region Public Methods

        /// <summary>
        /// Loads the ping services.
        /// </summary>
        /// <returns>A StringCollection.</returns>
        public override StringCollection LoadPingServices()
        {
            var fileName = this.Folder + "pingservices.xml";
            if (!File.Exists(fileName))
            {
                return new StringCollection();
            }

            var col = new StringCollection();
            var doc = new XmlDocument();
            doc.Load(fileName);

            foreach (XmlNode node in
                doc.SelectNodes("services/service").Cast<XmlNode>().Where(node => !col.Contains(node.InnerText)))
            {
                col.Add(node.InnerText);
            }

            return col;
        }

        /// <summary>
        /// Saves the ping services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public override void SavePingServices(StringCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            var fileName = this.Folder + "pingservices.xml";

            using (var writer = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.WriteStartDocument(true);
                writer.WriteStartElement("services");

                foreach (var service in services)
                {
                    writer.WriteElementString("service", service);
                }

                writer.WriteEndElement();
            }
        }

        #endregion
        #endregion

        #region Author Profiles
        #region Public Methods

        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object authorProfileSyncRoot = new object();

        /// <summary>
        /// The author profiles.
        /// </summary>
        private static List<AuthorProfile> authorProfiles;

        /// <summary>
        ///     Gets an unsorted list of all Author Profiles.
        /// </summary>
        /// <value>The categories.</value>
        public override List<AuthorProfile> Profiles
        {
            get
            {
                if (authorProfiles == null)
                {
                    lock (authorProfileSyncRoot)
                    {
                        if (authorProfiles == null)
                        {
                            authorProfiles = FillProfiles();
                            authorProfiles.Sort();
                        }
                    }
                }

                return authorProfiles;
            }
        }

        /// <summary>
        /// The delete profile.
        /// </summary>
        /// <param name="profile">
        /// The profile.
        /// </param>
        public override void DeleteProfile(AuthorProfile profile)
        {
            var fileName = string.Format("{0}profiles{1}{2}.xml", this.Folder, Path.DirectorySeparatorChar, profile.Id);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            if (AuthorProfile.All.Contains(profile))
            {
                AuthorProfile.All.Remove(profile);
            }
        }

        /// <summary>
        /// The fill profiles.
        /// </summary>
        /// <returns>
        /// A list of AuthorProfile.
        /// </returns>
        public override List<AuthorProfile> FillProfiles()
        {
            var folder = string.Format("{0}profiles{1}", Category.Folder, Path.DirectorySeparatorChar);

            return (from file in Directory.GetFiles(folder, "*.xml", SearchOption.TopDirectoryOnly)
                    select new FileInfo(file)
                        into info
                        select info.Name.Replace(".xml", string.Empty)
                            into username
                            select SelectProfile(username)).ToList();
        }

        /// <summary>
        /// The insert profile.
        /// </summary>
        /// <param name="profile">
        /// The profile.
        /// </param>
        public override void InsertProfile(AuthorProfile profile)
        {
            if (!Directory.Exists(string.Format("{0}profiles", this.Folder)))
            {
                Directory.CreateDirectory(string.Format("{0}profiles", this.Folder));
            }

            var fileName = string.Format("{0}profiles{1}{2}.xml", this.Folder, Path.DirectorySeparatorChar, profile.Id);
            var settings = new XmlWriterSettings { Indent = true };

            using (var writer = XmlWriter.Create(fileName, settings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("profileData");

                writer.WriteElementString("DisplayName", profile.DisplayName);
                writer.WriteElementString("FirstName", profile.FirstName);
                writer.WriteElementString("MiddleName", profile.MiddleName);
                writer.WriteElementString("LastName", profile.LastName);

                writer.WriteElementString("CityTown", profile.CityTown);
                writer.WriteElementString("RegionState", profile.RegionState);
                writer.WriteElementString("Country", profile.Country);

                writer.WriteElementString("Birthday", profile.Birthday.ToString("yyyy-MM-dd"));
                writer.WriteElementString("AboutMe", profile.AboutMe);
                writer.WriteElementString("PhotoURL", profile.PhotoUrl);

                writer.WriteElementString("Company", profile.Company);
                writer.WriteElementString("EmailAddress", profile.EmailAddress);
                writer.WriteElementString("PhoneMain", profile.PhoneMain);
                writer.WriteElementString("PhoneMobile", profile.PhoneMobile);
                writer.WriteElementString("PhoneFax", profile.PhoneFax);

                writer.WriteElementString("IsPrivate", profile.Private.ToString());

                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Retrieves a Page from the provider based on the specified id.
        /// </summary>
        /// <param name="id">The AuthorProfile id.</param>
        /// <returns>An AuthorProfile.</returns>
        public override AuthorProfile SelectProfile(string id)
        {
            var fileName = string.Format("{0}profiles{1}{2}.xml", this.Folder, Path.DirectorySeparatorChar, id);
            var doc = new XmlDocument();
            doc.Load(fileName);

            var profile = new AuthorProfile(id);

            if (doc.SelectSingleNode("//DisplayName") != null)
            {
                profile.DisplayName = doc.SelectSingleNode("//DisplayName").InnerText;
            }

            if (doc.SelectSingleNode("//FirstName") != null)
            {
                profile.FirstName = doc.SelectSingleNode("//FirstName").InnerText;
            }

            if (doc.SelectSingleNode("//MiddleName") != null)
            {
                profile.MiddleName = doc.SelectSingleNode("//MiddleName").InnerText;
            }

            if (doc.SelectSingleNode("//LastName") != null)
            {
                profile.LastName = doc.SelectSingleNode("//LastName").InnerText;
            }

            // profile.Address1 = doc.SelectSingleNode("//Address1").InnerText;
            // profile.Address2 = doc.SelectSingleNode("//Address2").InnerText;
            if (doc.SelectSingleNode("//CityTown") != null)
            {
                profile.CityTown = doc.SelectSingleNode("//CityTown").InnerText;
            }

            if (doc.SelectSingleNode("//RegionState") != null)
            {
                profile.RegionState = doc.SelectSingleNode("//RegionState").InnerText;
            }

            if (doc.SelectSingleNode("//Country") != null)
            {
                profile.Country = doc.SelectSingleNode("//Country").InnerText;
            }

            if (doc.SelectSingleNode("//Birthday") != null)
            {
                DateTime date;
                if (DateTime.TryParse(doc.SelectSingleNode("//Birthday").InnerText, out date))
                {
                    profile.Birthday = date;
                }
            }

            if (doc.SelectSingleNode("//AboutMe") != null)
            {
                profile.AboutMe = doc.SelectSingleNode("//AboutMe").InnerText;
            }

            if (doc.SelectSingleNode("//PhotoURL") != null)
            {
                profile.PhotoUrl = doc.SelectSingleNode("//PhotoURL").InnerText;
            }

            if (doc.SelectSingleNode("//Company") != null)
            {
                profile.Company = doc.SelectSingleNode("//Company").InnerText;
            }

            if (doc.SelectSingleNode("//EmailAddress") != null)
            {
                profile.EmailAddress = doc.SelectSingleNode("//EmailAddress").InnerText;
            }

            if (doc.SelectSingleNode("//PhoneMain") != null)
            {
                profile.PhoneMain = doc.SelectSingleNode("//PhoneMain").InnerText;
            }

            if (doc.SelectSingleNode("//PhoneMobile") != null)
            {
                profile.PhoneMobile = doc.SelectSingleNode("//PhoneMobile").InnerText;
            }

            if (doc.SelectSingleNode("//PhoneFax") != null)
            {
                profile.PhoneFax = doc.SelectSingleNode("//PhoneFax").InnerText;
            }

            if (doc.SelectSingleNode("//IsPrivate") != null)
            {
                profile.Private = doc.SelectSingleNode("//IsPrivate").InnerText == "true";
            }

            // page.DateCreated = DateTime.Parse(doc.SelectSingleNode("page/datecreated").InnerText, CultureInfo.InvariantCulture);
            // page.DateModified = DateTime.Parse(doc.SelectSingleNode("page/datemodified").InnerText, CultureInfo.InvariantCulture);
            return profile;
        }

        /// <summary>
        /// The update profile.
        /// </summary>
        /// <param name="profile">
        /// The profile.
        /// </param>
        public override void UpdateProfile(AuthorProfile profile)
        {
            this.InsertProfile(profile);
        }

        #endregion
        #endregion

        #region Profile
        /*
        ///<summary>
        ///</summary>
        ///<param name="username"></param>
        ///<returns></returns>
        public override Profile SelectProfile(string username)
        {
            Profile profile = new Profile();
            try
            {
                // Deserialize the specified file to a Theater object.
                XmlSerializer xs = new XmlSerializer(typeof(Profile));
                FileStream fs =
                    new FileStream(BlogSettings.Instance.StorageLocation + username.ToLowerInvariant() + ".xml",
                                   FileMode.Open);
                profile = (Profile)xs.Deserialize(fs);
            }
            catch (Exception x)
            {
                Console.WriteLine("Exception: " + x.Message);
            }
            return profile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userProfile"></param>
        public override void InsertProfile(Profile userProfile)
        {
            try
            {
                // Serialize the Profile object to an XML file.
                XmlSerializer xs = new XmlSerializer(typeof(Profile));
                FileStream fs =
                    new FileStream(
                        BlogSettings.Instance.StorageLocation + userProfile.userName.ToLowerInvariant() + ".xml",
                        FileMode.Create);
                xs.Serialize(fs, userProfile);
            }
            catch (Exception x)
            {
                Console.WriteLine("Exception: " + x.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //public override List<Profile> FillProfiles()
        //{
        //    List<Profile> profiles = new List<Profile>();
        //    foreach (MembershipUser user in Membership.GetAllUsers())
        //    {
        //        Profile userProfile = GetProfile(user.UserName);
        //        profiles.Add(userProfile);
        //    }
        //    return profiles;
        //}

        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object profileSyncRoot = new object();

        /// <summary>
        /// The author profiles.
        /// </summary>
        private static List<Profile> profiles;

        /// <summary>
        ///     Gets an unsorted list of all Author Profiles.
        /// </summary>
        /// <value>The categories.</value>
        public override List<Profile> Profiles
        {
            get
            {
                if (profiles == null)
                {
                    lock (profileSyncRoot)
                    {
                        if (profiles == null)
                        {
                            profiles = FillProfiles();
                            profiles.Sort();
                        }
                    }
                }

                return profiles;
            }
        }
         */
        #endregion

        #region Referrers
        #region Public Methods
        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object referrerSyncRoot = new object();

        /// <summary>
        /// The blog rolls.
        /// </summary>
        private static List<Referrer> referrers;

        public override List<Referrer> Referrers
        {
            get
            {
                if (referrers == null || referrers.Count == 0)
                {
                    lock (referrerSyncRoot)
                    {
                        if (referrers == null || referrers.Count == 0)
                        {
                            referrers = this.FillReferrers();
                            referrers.Sort();
                        }
                    }
                }
                return referrers;
            }
        }

        /// <summary>
        /// Fills an unsorted list of Referrers.
        /// </summary>
        /// <returns>
        /// A List&lt;Referrer&gt; of all Referrers.
        /// </returns>
        public override List<Referrer> FillReferrers()
        {
            var folder = Path.Combine(this.Folder, "log");

            var referrers = new List<Referrer>();
            var oldFileDate = DateTime.Today.AddDays(-BlogSettings.Instance.NumberOfReferrerDays);

            var dirInfo = new DirectoryInfo(folder);
            if (dirInfo.Exists)
            {
                var logFiles = new List<FileInfo>(dirInfo.GetFiles());
                foreach (var file in logFiles)
                {
                    var fileName = file.Name.Replace(".xml", string.Empty);
                    var dateStrings = fileName.Split(new[] { '.' });
                    if (dateStrings.Length != 3)
                    {
                        file.Delete();
                        continue;
                    }

                    var day = new DateTime(
                        int.Parse(dateStrings[0]), int.Parse(dateStrings[1]), int.Parse(dateStrings[2]));
                    if (day < oldFileDate)
                    {
                        file.Delete();
                        continue;
                    }

                    referrers.AddRange(GetReferrersFromFile(file, day));
                }
            }

            return referrers;
        }

        /// <summary>
        /// Inserts a Referrer.
        /// </summary>
        /// <param name="referrer">
        /// Must be a valid Referrer object.
        /// </param>
        public override void InsertReferrer(Referrer referrer)
        {
            Referrer.All.Add(referrer);

            referrer.MarkOld();
            var day = Referrer.All.FindAll(r => r.Day.ToShortDateString() == referrer.Day.ToShortDateString());
            this.WriteReferrerFile(day, referrer.Day);
        }

        /// <summary>
        /// Gets a Referrer based on the Id.
        /// </summary>
        /// <param name="Id">
        /// The Referrer's Id.
        /// </param>
        /// <returns>
        /// A matching Referrer.
        /// </returns>
        public override Referrer SelectReferrer(Guid Id)
        {
            var refer = Referrer.All.Find(r => r.Id.Equals(Id)) ?? new Referrer();

            refer.MarkOld();
            return refer;
        }

        /// <summary>
        /// Updates a Referrer.
        /// </summary>
        /// <param name="referrer">
        /// Must be a valid Referrer object.
        /// </param>
        public override void UpdateReferrer(Referrer referrer)
        {
            var day = Referrer.All.FindAll(r => r.Day.ToShortDateString() == referrer.Day.ToShortDateString());
            this.WriteReferrerFile(day, referrer.Day);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get referrers from file.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="day">
        /// The day.
        /// </param>
        /// <returns>
        /// A list of Referrer.
        /// </returns>
        private static IEnumerable<Referrer> GetReferrersFromFile(FileInfo file, DateTime day)
        {
            var referrers = new List<Referrer>();

            var doc = new XmlDocument();
            doc.Load(file.FullName);

            var nodes = doc.SelectNodes("referrers/referrer");
            if (nodes != null)
            {
                foreach (var refer in
                    nodes.Cast<XmlNode>().Select(
                        node =>
                        new Referrer
                        {
                            Url = node.Attributes["url"] == null ? null : new Uri(node.Attributes["url"].InnerText),
                            Count =
                                node.Attributes["count"] == null ? 0 : int.Parse(node.Attributes["count"].InnerText),
                            Day = day,
                            PossibleSpam =
                                node.Attributes["isSpam"] == null
                                    ? false
                                    : bool.Parse(node.Attributes["isSpam"].InnerText),
                            ReferrerUrl = new Uri(node.InnerText),
                            Id = Guid.NewGuid()
                        }))
                {
                    refer.MarkOld();
                    referrers.Add(refer);
                }
            }

            return referrers;
        }

        /// <summary>
        /// The write referrer file.
        /// </summary>
        /// <param name="referrers">
        /// The referrers.
        /// </param>
        /// <param name="day">
        /// The day.
        /// </param>
        private void WriteReferrerFile(List<Referrer> referrers, DateTime day)
        {
            var folder = Path.Combine(this.Folder, "log");
            var fileName = Path.Combine(folder, string.Format("{0}.xml", day.ToString("yyyy.MM.dd")));
            var dirInfo = new DirectoryInfo(folder);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            using (var writer = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.WriteStartDocument(true);
                writer.WriteStartElement("referrers");

                foreach (var refer in referrers)
                {
                    writer.WriteStartElement("referrer");
                    writer.WriteAttributeString("url", refer.Url.ToString());
                    writer.WriteAttributeString("count", refer.Count.ToString());
                    writer.WriteAttributeString("isSpam", refer.PossibleSpam.ToString());
                    writer.WriteString(refer.ReferrerUrl.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }

        #endregion
        #endregion

        #region StopWords
        #region Public Methods

        /// <summary>
        /// Loads the stop words used in the search feature.
        /// </summary>
        /// <returns>
        /// A StringCollection.
        /// </returns>
        public override StringCollection LoadStopWords()
        {
            var fileName = string.Format("{0}stopwords.txt", this.Folder);
            if (!File.Exists(fileName))
            {
                return new StringCollection();
            }

            using (var reader = new StreamReader(fileName))
            {
                var file = reader.ReadToEnd();
                var words = file.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                var col = new StringCollection();
                col.AddRange(words);

                return col;
            }
        }

        #endregion
        #endregion

        #region Settings
        #region Public Methods

        /// <summary>
        /// Loads the settings from the provider.
        /// </summary>
        /// <returns>A StringDictionary.</returns>
        public override StringDictionary LoadSettings()
        {
            var filename = HttpContext.Current.Server.MapPath(string.Format("{0}settings.xml", this.StorageLocation()));
            var dic = new StringDictionary();

            var doc = new XmlDocument();
            doc.Load(filename);

            var settings = doc.SelectSingleNode("settings");
            if (settings != null)
            {
                foreach (XmlNode settingsNode in settings.ChildNodes)
                {
                    var name = settingsNode.Name;
                    var value = settingsNode.InnerText;

                    dic.Add(name, value);
                }
            }

            return dic;
        }

        /// <summary>
        /// Saves the settings to the provider.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public override void SaveSettings(StringDictionary settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            var filename = string.Format("{0}settings.xml", this.Folder);
            var writerSettings = new XmlWriterSettings { Indent = true };

            // ------------------------------------------------------------
            // Create XML writer against file path
            // ------------------------------------------------------------
            using (var writer = XmlWriter.Create(filename, writerSettings))
            {
                writer.WriteStartElement("settings");

                foreach (string key in settings.Keys)
                {
                    writer.WriteElementString(key, settings[key]);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// The storage location is to allow Blog Providers to use alternative storage locations that app_data root directory.
        /// </summary>
        /// <returns>
        /// The storage location.
        /// </returns>
        public override string StorageLocation()
        {
            return String.IsNullOrEmpty(WebConfigurationManager.AppSettings["StorageLocation"]) ? @"~/app_data/" : WebConfigurationManager.AppSettings["StorageLocation"];
        }

        #endregion
        #endregion

        #region DataStore
       
        #region Public Methods
        /*

        /// <summary>
        /// Loads settings from generic data store
        /// </summary>
        /// <param name="extensionType">
        /// Extension Type
        /// </param>
        /// <param name="extensionId">
        /// Extension ID
        /// </param>
        /// <returns>
        /// Stream Settings
        /// </returns>
        public override object LoadFromDataStore(ExtensionType extensionType, string extensionId)
        {
            var fileName = string.Format("{0}{1}.xml", StorageLocation(extensionType), extensionId);
            Stream str = null;
            if (!Directory.Exists(StorageLocation(extensionType)))
            {
                Directory.CreateDirectory(StorageLocation(extensionType));
            }

            if (File.Exists(fileName))
            {
                var reader = new StreamReader(fileName);
                str = reader.BaseStream;
            }

            return str;
        }

        /// <summary>
        /// Removes settings from data store
        /// </summary>
        /// <param name="extensionType">
        /// Extension Type
        /// </param>
        /// <param name="extensionId">
        /// Extension Id
        /// </param>
        public override void RemoveFromDataStore(ExtensionType extensionType, string extensionId)
        {
            var fileName = string.Format("{0}{1}.xml", StorageLocation(extensionType), extensionId);
            File.Delete(fileName);
        }

        /// <summary>
        /// Save settings to generic data store
        /// </summary>
        /// <param name="extensionType">
        /// Type of extension
        /// </param>
        /// <param name="extensionId">
        /// Extension ID
        /// </param>
        /// <param name="settings">
        /// Stream Settings
        /// </param>
        public override void SaveToDataStore(ExtensionType extensionType, string extensionId, object settings)
        {
            var fileName = string.Format("{0}{1}.xml", StorageLocation(extensionType), extensionId);
            if (!Directory.Exists(StorageLocation(extensionType)))
            {
                Directory.CreateDirectory(StorageLocation(extensionType));
            }

            using (TextWriter writer = new StreamWriter(fileName))
            {
                var x = new XmlSerializer(settings.GetType());
                x.Serialize(writer, settings);
            }
        }
        */
        #endregion

        #region Methods
        /*
        /// <summary>
        /// Data Store Location
        /// </summary>
        /// <param name="extensionType">
        /// Type of extension
        /// </param>
        /// <returns>
        /// Path to storage directory
        /// </returns>
        private static string StorageLocation(ExtensionType extensionType)
        {
            switch (extensionType)
            {
                case ExtensionType.Extension:
                    return
                        HostingEnvironment.MapPath(
                            Path.Combine(BlogSettings.Instance.StorageLocation, @"datastore\extensions\"));
                case ExtensionType.Widget:
                    return
                        HostingEnvironment.MapPath(
                            Path.Combine(BlogSettings.Instance.StorageLocation, @"datastore\widgets\"));
                case ExtensionType.Theme:
                    return
                        HostingEnvironment.MapPath(
                            Path.Combine(BlogSettings.Instance.StorageLocation, @"datastore\themes\"));
            }

            return string.Empty;
        }
        */
        #endregion
         
        #endregion
    }
}
