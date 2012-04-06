namespace BlogEngine.Core.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Web.Configuration;
    using System.Xml.Serialization;

    using BlogEngine.Core.DataStore;

    /// <summary>
    /// Generic Database BlogProvider
    /// </summary>
    public class DbBlogProvider : BlogProvider
    {
        #region Constants and Fields

        /// <summary>
        /// The conn string name.
        /// </summary>
        private string connStringName;

        /// <summary>
        /// The parm prefix.
        /// </summary>
        private string parmPrefix;

        /// <summary>
        /// The table prefix.
        /// </summary>
        private string tablePrefix;

        #endregion

        #region Public Methods

        /// <summary>
        /// Deletes a BlogRoll from the database
        /// </summary>
        /// <param name="blogRollItem">
        /// The blog Roll Item.
        /// </param>
        public override void DeleteBlogRollItem(BlogRollItem blogRollItem)
        {
            var blogRolls = BlogRollItem.BlogRolls;
            blogRolls.Remove(blogRollItem);
            blogRolls.Add(blogRollItem);

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("DELETE FROM {0}BlogRollItems WHERE BlogRollId = {1}BlogRollId", this.tablePrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var parameter = provider.CreateParameter();
                    if (parameter != null)
                    {
                        parameter.ParameterName = string.Format("{0}BlogRollId", this.parmPrefix);
                        parameter.Value = blogRollItem.Id.ToString();
                        cmd.Parameters.Add(parameter);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Deletes a category from the database
        /// </summary>
        /// <param name="category">
        /// category to be removed
        /// </param>
        public override void DeleteCategory(Category category)
        {
            var categories = Category.Categories;
            categories.Remove(category);

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("DELETE FROM {0}PostCategory WHERE CategoryID = {1}catid;DELETE FROM {2}Categories WHERE CategoryID = {3}catid", this.tablePrefix, this.parmPrefix, this.tablePrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var id = provider.CreateParameter();
                    if (id != null)
                    {
                        id.ParameterName = string.Format("{0}catid", this.parmPrefix);
                        id.Value = category.Id.ToString();
                        cmd.Parameters.Add(id);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Deletes a page from the database
        /// </summary>
        /// <param name="page">
        /// page to be deleted
        /// </param>
        public override void DeletePage(Page page)
        {
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("DELETE FROM {0}Pages WHERE PageID = {1}id", this.tablePrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var id = provider.CreateParameter();
                    if (id != null)
                    {
                        id.ParameterName = string.Format("{0}id", this.parmPrefix);
                        id.Value = page.Id.ToString();
                        cmd.Parameters.Add(id);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Deletes a post in the database
        /// </summary>
        /// <param name="post">
        /// post to delete
        /// </param>
        public override void DeletePost(Post post)
        {
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("DELETE FROM {0}PostTag WHERE PostID = @id;DELETE FROM {1}PostCategory WHERE PostID = @id;DELETE FROM {2}PostNotify WHERE PostID = @id;DELETE FROM {3}PostComment WHERE PostID = @id;DELETE FROM {4}Posts WHERE PostID = @id;", this.tablePrefix, this.tablePrefix, this.tablePrefix, this.tablePrefix, this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var id = provider.CreateParameter();
                    if (id != null)
                    {
                        id.ParameterName = string.Format("{0}id", this.parmPrefix);
                        id.Value = post.Id.ToString();
                        cmd.Parameters.Add(id);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Remove AuthorProfile from database
        /// </summary>
        /// <param name="profile">An AuthorProfile.</param>
        public override void DeleteProfile(AuthorProfile profile)
        {
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("DELETE FROM {0}Profiles WHERE UserName = {1}name", this.tablePrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var nameDp = provider.CreateParameter();
                    if (nameDp != null)
                    {
                        nameDp.ParameterName = string.Format("{0}name", this.parmPrefix);
                        nameDp.Value = profile.Id;
                        cmd.Parameters.Add(nameDp);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Gets all BlogRolls in database
        /// </summary>
        /// <returns>
        /// List of BlogRolls
        /// </returns>
        public override List<BlogRollItem> FillBlogRoll()
        {
            var blogRoll = new List<BlogRollItem>();

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn != null)
                {
                    conn.ConnectionString = connString;
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        var sqlQuery = string.Format("SELECT BlogRollId, Title, Description, BlogUrl, FeedUrl, Xfn, SortIndex FROM {0}BlogRollItems ", this.tablePrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    var br = new BlogRollItem
                                        {
                                            Id = rdr.GetGuid(0),
                                            Title = rdr.GetString(1),
                                            Description = rdr.IsDBNull(2) ? string.Empty : rdr.GetString(2),
                                            BlogUrl = rdr.IsDBNull(3) ? null : new Uri(rdr.GetString(3)),
                                            FeedUrl = rdr.IsDBNull(4) ? null : new Uri(rdr.GetString(4)),
                                            Xfn = rdr.IsDBNull(5) ? string.Empty : rdr.GetString(5),
                                            SortIndex = rdr.GetInt32(6)
                                        };

                                    blogRoll.Add(br);
                                    br.MarkOld();
                                }
                            }
                        }
                    }
                }
            }

            return blogRoll;
        }

        /// <summary>
        /// Gets all categories in database
        /// </summary>
        /// <returns>
        /// List of categories
        /// </returns>
        public override List<Category> FillCategories()
        {
            var categories = new List<Category>();
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn != null)
                {
                    conn.ConnectionString = connString;

                    using (var cmd = conn.CreateCommand())
                    {
                        var sqlQuery = string.Format("SELECT CategoryID, CategoryName, description, ParentID FROM {0}Categories ", this.tablePrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();

                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    var cat = new Category
                                        {
                                            Title = rdr.GetString(1),
                                            Description = rdr.IsDBNull(2) ? string.Empty : rdr.GetString(2),
                                            Parent = rdr.IsDBNull(3) ? (Guid?)null : new Guid(rdr.GetGuid(3).ToString()),
                                            Id = new Guid(rdr.GetGuid(0).ToString())
                                        };

                                    categories.Add(cat);
                                    cat.MarkOld();
                                }
                            }
                        }
                    }
                }
            }

            return categories;
        }

        /// <summary>
        /// Gets all pages in database
        /// </summary>
        /// <returns>
        /// List of pages
        /// </returns>
        public override List<Page> FillPages()
        {
            var pageIDs = new List<string>();
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn != null)
                {
                    conn.ConnectionString = connString;

                    using (var cmd = conn.CreateCommand())
                    {
                        var sqlQuery = string.Format("SELECT PageID FROM {0}Pages ", this.tablePrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        conn.Open();

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                pageIDs.Add(rdr.GetGuid(0).ToString());
                            }
                        }
                    }
                }
            }

            return pageIDs.Select(id => Page.Load(new Guid(id))).ToList();
        }

        /// <summary>
        /// Gets all post from the database
        /// </summary>
        /// <returns>
        /// List of posts
        /// </returns>
        public override List<Post> FillPosts()
        {
            var postIDs = new List<string>();
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn != null)
                {
                    conn.ConnectionString = connString;

                    using (var cmd = conn.CreateCommand())
                    {
                        var sqlQuery = string.Format("SELECT PostID FROM {0}Posts ", this.tablePrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        conn.Open();

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                postIDs.Add(rdr.GetGuid(0).ToString());
                            }
                        }
                    }
                }
            }

            var posts = postIDs.Select(id => Post.Load(new Guid(id))).ToList();

            posts.Sort();
            return posts;
        }

        /// <summary>
        /// Return collection for AuthorProfiles from database
        /// </summary>
        /// <returns>
        /// List of AuthorProfile
        /// </returns>
        public override List<AuthorProfile> FillProfiles()
        {
            var profileNames = new List<string>();
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn != null)
                {
                    conn.ConnectionString = connString;
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        var sqlQuery = string.Format("SELECT UserName FROM {0}Profiles GROUP BY UserName", this.tablePrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                profileNames.Add(rdr.GetString(0));
                            }
                        }
                    }
                }
            }

            return profileNames.Select(BusinessBase<AuthorProfile, string>.Load).ToList();
        }

        /// <summary>
        /// Gets all Referrers from the database.
        /// </summary>
        /// <returns>
        /// List of Referrers.
        /// </returns>
        public override List<Referrer> FillReferrers()
        {
            this.DeleteOldReferrers();

            var referrers = new List<Referrer>();

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn != null)
                {
                    conn.ConnectionString = connString;
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        var sqlQuery = string.Format("SELECT ReferrerId, ReferralDay, ReferrerUrl, ReferralCount, Url, IsSpam FROM {0}Referrers ", this.tablePrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    var refer = new Referrer
                                        {
                                            Id = rdr.GetGuid(0),
                                            Day = rdr.GetDateTime(1),
                                            ReferrerUrl = new Uri(rdr.GetString(2)),
                                            Count = rdr.GetInt32(3),
                                            Url = rdr.IsDBNull(4) ? null : new Uri(rdr.GetString(4)),
                                            PossibleSpam = rdr.IsDBNull(5) ? false : rdr.GetBoolean(5)
                                        };

                                    referrers.Add(refer);
                                    refer.MarkOld();
                                }
                            }
                        }
                    }
                }
            }

            return referrers;
        }

        /// <summary>
        /// Initializes the provider
        /// </summary>
        /// <param name="name">
        /// Configuration name
        /// </param>
        /// <param name="config">
        /// Configuration settings
        /// </param>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (String.IsNullOrEmpty(name))
            {
                name = "DbBlogProvider";
            }

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Generic Database Blog Provider");
            }

            base.Initialize(name, config);

            if (config["connectionStringName"] == null)
            {
                // default to BlogEngine
                config["connectionStringName"] = "BlogEngine";
            }

            this.connStringName = config["connectionStringName"];
            config.Remove("connectionStringName");

            if (config["tablePrefix"] == null)
            {
                // default
                config["tablePrefix"] = "be_";
            }

            this.tablePrefix = config["tablePrefix"];
            config.Remove("tablePrefix");

            if (config["parmPrefix"] == null)
            {
                // default
                config["parmPrefix"] = "@";
            }

            this.parmPrefix = config["parmPrefix"];
            config.Remove("parmPrefix");

            // Throw an exception if unrecognized attributes remain
            if (config.Count > 0)
            {
                var attr = config.GetKey(0);
                if (!String.IsNullOrEmpty(attr))
                {
                    throw new ProviderException(string.Format("Unrecognized attribute: {0}", attr));
                }
            }
        }

        /// <summary>
        /// Adds a new BlogRoll to the database.
        /// </summary>
        /// <param name="blogRollItem">
        /// The blog Roll Item.
        /// </param>
        public override void InsertBlogRollItem(BlogRollItem blogRollItem)
        {
            var blogRolls = BlogRollItem.BlogRolls;
            blogRolls.Add(blogRollItem);

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("INSERT INTO {0}BlogRollItems (BlogRollId, Title, Description, BlogUrl, FeedUrl, Xfn, SortIndex) VALUES (@BlogRollId, @Title, @Description, @BlogUrl, @FeedUrl, @Xfn, @SortIndex)", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    this.AddBlogRollParametersToCommand(blogRollItem, provider, cmd);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Adds a new category to the database
        /// </summary>
        /// <param name="category">
        /// category to add
        /// </param>
        public override void InsertCategory(Category category)
        {
            var categories = Category.Categories;
            categories.Add(category);

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("INSERT INTO {0}Categories (CategoryID, CategoryName, description, ParentID) VALUES (@catid, @catname, @description, @parentid)", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var id = provider.CreateParameter();
                    if (id != null)
                    {
                        id.ParameterName = this.parmPrefix + "catid";
                        id.Value = category.Id.ToString();
                        cmd.Parameters.Add(id);
                    }

                    var title = provider.CreateParameter();
                    if (title != null)
                    {
                        title.ParameterName = this.parmPrefix + "catname";
                        title.Value = category.Title;
                        cmd.Parameters.Add(title);
                    }

                    var desc = provider.CreateParameter();
                    if (desc != null)
                    {
                        desc.ParameterName = this.parmPrefix + "description";
                        desc.Value = category.Description;
                        cmd.Parameters.Add(desc);
                    }

                    var parent = provider.CreateParameter();
                    if (parent != null)
                    {
                        parent.ParameterName = this.parmPrefix + "parentid";
                        parent.Value = category.Parent == null ? (object)DBNull.Value : category.Parent.ToString();

                        cmd.Parameters.Add(parent);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Adds a page to the database
        /// </summary>
        /// <param name="page">
        /// page to be added
        /// </param>
        public override void InsertPage(Page page)
        {
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("INSERT INTO {0}Pages (PageID, Title, Description, PageContent, DateCreated, DateModified, Keywords, IsPublished, IsFrontPage, Parent, ShowInList, Slug) VALUES (@id, @title, @desc, @content, @created, @modified, @keywords, @ispublished, @isfrontpage, @parent, @showinlist, @slug)", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var id = provider.CreateParameter();
                    if (id != null)
                    {
                        id.ParameterName = string.Format("{0}id", this.parmPrefix);
                        id.Value = page.Id.ToString();
                        cmd.Parameters.Add(id);
                    }

                    var title = provider.CreateParameter();
                    if (title != null)
                    {
                        title.ParameterName = string.Format("{0}title", this.parmPrefix);
                        title.Value = page.Title;
                        cmd.Parameters.Add(title);
                    }

                    var desc = provider.CreateParameter();
                    if (desc != null)
                    {
                        desc.ParameterName = string.Format("{0}desc", this.parmPrefix);
                        desc.Value = page.Description;
                        cmd.Parameters.Add(desc);
                    }

                    var content = provider.CreateParameter();
                    if (content != null)
                    {
                        content.ParameterName = string.Format("{0}content", this.parmPrefix);
                        content.Value = page.Content;
                        cmd.Parameters.Add(content);
                    }

                    var created = provider.CreateParameter();
                    if (created != null)
                    {
                        created.ParameterName = string.Format("{0}created", this.parmPrefix);
                        created.Value = page.DateCreated.AddHours(-BlogSettings.Instance.Timezone);
                        cmd.Parameters.Add(created);
                    }

                    var modified = provider.CreateParameter();
                    if (modified != null)
                    {
                        modified.ParameterName = string.Format("{0}modified", this.parmPrefix);
                        modified.Value = page.DateModified == new DateTime() ? DateTime.Now : page.DateModified.AddHours(-BlogSettings.Instance.Timezone);
                        cmd.Parameters.Add(modified);
                    }

                    var keywords = provider.CreateParameter();
                    if (keywords != null)
                    {
                        keywords.ParameterName = string.Format("{0}keywords", this.parmPrefix);
                        keywords.Value = page.Keywords;
                        cmd.Parameters.Add(keywords);
                    }

                    var published = provider.CreateParameter();
                    if (published != null)
                    {
                        published.ParameterName = string.Format("{0}ispublished", this.parmPrefix);
                        published.Value = page.Published;
                        cmd.Parameters.Add(published);
                    }

                    var frontPage = provider.CreateParameter();
                    if (frontPage != null)
                    {
                        frontPage.ParameterName = string.Format("{0}isfrontpage", this.parmPrefix);
                        frontPage.Value = page.FrontPage;
                        cmd.Parameters.Add(frontPage);
                    }

                    var parent = provider.CreateParameter();
                    if (parent != null)
                    {
                        parent.ParameterName = string.Format("{0}parent", this.parmPrefix);
                        parent.Value = page.Parent.ToString();
                        cmd.Parameters.Add(parent);
                    }

                    var showInList = provider.CreateParameter();
                    if (showInList != null)
                    {
                        showInList.ParameterName = string.Format("{0}showinlist", this.parmPrefix);
                        showInList.Value = page.ShowInList;
                        cmd.Parameters.Add(showInList);
                    }

                    var slug = provider.CreateParameter();
                    if (slug != null)
                    {
                        slug.ParameterName = string.Format("{0}slug", this.parmPrefix);
                        slug.Value = page.Slug;
                        cmd.Parameters.Add(slug);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Adds a new post to database
        /// </summary>
        /// <param name="post">
        /// The new post.
        /// </param>
        public override void InsertPost(Post post)
        {
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("INSERT INTO {0}Posts (PostID, Title, Description, PostContent, DateCreated, DateModified, Author, IsPublished, IsCommentEnabled, Raters, Rating, Slug)VALUES (@id, @title, @desc, @content, @created, @modified, @author, @published, @commentEnabled, @raters, @rating, @slug)", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var dpID = provider.CreateParameter();
                    dpID.ParameterName = this.parmPrefix + "id";
                    dpID.Value = post.Id.ToString();
                    cmd.Parameters.Add(dpID);

                    var dpTitle = provider.CreateParameter();
                    dpTitle.ParameterName = this.parmPrefix + "title";
                    dpTitle.Value = post.Title;
                    cmd.Parameters.Add(dpTitle);

                    var dpDesc = provider.CreateParameter();
                    dpDesc.ParameterName = this.parmPrefix + "desc";
                    dpDesc.Value = post.Description ?? string.Empty;
                    cmd.Parameters.Add(dpDesc);

                    var dpContent = provider.CreateParameter();
                    dpContent.ParameterName = this.parmPrefix + "content";
                    dpContent.Value = post.Content;
                    cmd.Parameters.Add(dpContent);

                    var dpCreated = provider.CreateParameter();
                    dpCreated.ParameterName = this.parmPrefix + "created";
                    dpCreated.Value = post.DateCreated.AddHours(-BlogSettings.Instance.Timezone);
                    cmd.Parameters.Add(dpCreated);

                    var dpModified = provider.CreateParameter();
                    dpModified.ParameterName = this.parmPrefix + "modified";
                    dpModified.Value = post.DateModified == new DateTime() ? DateTime.Now : post.DateModified.AddHours(-BlogSettings.Instance.Timezone);
                    cmd.Parameters.Add(dpModified);

                    var dpAuthor = provider.CreateParameter();
                    dpAuthor.ParameterName = this.parmPrefix + "author";
                    dpAuthor.Value = post.Author ?? string.Empty;
                    cmd.Parameters.Add(dpAuthor);

                    var dpPublished = provider.CreateParameter();
                    dpPublished.ParameterName = this.parmPrefix + "published";
                    dpPublished.Value = post.Published;
                    cmd.Parameters.Add(dpPublished);

                    var dpCommentEnabled = provider.CreateParameter();
                    dpCommentEnabled.ParameterName = this.parmPrefix + "commentEnabled";
                    dpCommentEnabled.Value = post.HasCommentsEnabled;
                    cmd.Parameters.Add(dpCommentEnabled);

                    var dpRaters = provider.CreateParameter();
                    dpRaters.ParameterName = this.parmPrefix + "raters";
                    dpRaters.Value = post.Raters;
                    cmd.Parameters.Add(dpRaters);

                    var dpRating = provider.CreateParameter();
                    dpRating.ParameterName = this.parmPrefix + "rating";
                    dpRating.Value = post.Rating;
                    cmd.Parameters.Add(dpRating);

                    var dpSlug = provider.CreateParameter();
                    dpSlug.ParameterName = this.parmPrefix + "slug";
                    dpSlug.Value = post.Slug ?? string.Empty;
                    cmd.Parameters.Add(dpSlug);

                    cmd.ExecuteNonQuery();
                }

                // Tags
                this.UpdateTags(post, conn, provider);

                // Categories
                this.UpdateCategories(post, conn, provider);

                // Comments
                this.UpdateComments(post, conn, provider);

                // Email Notification
                this.UpdateNotify(post, conn, provider);
            }
        }

        /// <summary>
        /// Adds AuthorProfile to database
        /// </summary>
        /// <param name="profile">An AuthorProfile.</param>
        public override void InsertProfile(AuthorProfile profile)
        {
            this.UpdateProfile(profile);
        }

        /// <summary>
        /// Adds a new Referrer to the database.
        /// </summary>
        /// <param name="referrer">
        /// Referrer to add.
        /// </param>
        public override void InsertReferrer(Referrer referrer)
        {
            var referrers = Referrer.Referrers;
            referrers.Add(referrer);

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("INSERT INTO {0}Referrers (ReferrerId, ReferralDay, ReferrerUrl, ReferralCount, Url, IsSpam) VALUES (@ReferrerId, @ReferralDay, @ReferrerUrl, @ReferralCount, @Url, @IsSpam)", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    this.AddReferrersParametersToCommand(referrer, provider, cmd);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Load user data from DataStore
        /// </summary>
        /// <param name="extensionType">
        /// type of info
        /// </param>
        /// <param name="extensionId">
        /// id of info
        /// </param>
        /// <returns>
        /// stream of detail data
        /// </returns>
        public override object LoadFromDataStore(ExtensionType extensionType, string extensionId)
        {
            // MemoryStream stream;
            object o = null;
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return o;
                }

                conn.ConnectionString = connString;

                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("SELECT Settings FROM {0}DataStoreSettings WHERE ExtensionType = {1}etype AND ExtensionId = {2}eid", this.tablePrefix, this.parmPrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;
                    conn.Open();

                    var dpeType = provider.CreateParameter();
                    if (dpeType != null)
                    {
                        dpeType.ParameterName = string.Format("{0}etype", this.parmPrefix);
                        dpeType.Value = extensionType.GetHashCode();
                        cmd.Parameters.Add(dpeType);
                    }

                    var dpeId = provider.CreateParameter();
                    if (dpeId != null)
                    {
                        dpeId.ParameterName = string.Format("{0}eid", this.parmPrefix);
                        dpeId.Value = extensionId;
                        cmd.Parameters.Add(dpeId);
                    }

                    o = cmd.ExecuteScalar();
                }
            }

            return o;
        }

        /// <summary>
        /// Gets the PingServices from the database
        /// </summary>
        /// <returns>
        /// collection of PingServices
        /// </returns>
        public override StringCollection LoadPingServices()
        {
            var col = new StringCollection();
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn != null)
                {
                    conn.ConnectionString = connString;

                    using (var cmd = conn.CreateCommand())
                    {
                        var sqlQuery = string.Format("SELECT Link FROM {0}PingService", this.tablePrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                if (!col.Contains(rdr.GetString(0)))
                                {
                                    col.Add(rdr.GetString(0));
                                }
                            }
                        }
                    }
                }
            }

            return col;
        }

        /// <summary>
        /// Gets the settings from the database
        /// </summary>
        /// <returns>
        /// dictionary of settings
        /// </returns>
        public override StringDictionary LoadSettings()
        {
            var dic = new StringDictionary();
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn != null)
                {
                    conn.ConnectionString = connString;

                    using (var cmd = conn.CreateCommand())
                    {
                        var sqlQuery = string.Format("SELECT SettingName, SettingValue FROM {0}Settings", this.tablePrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                var name = rdr.GetString(0);
                                var value = rdr.GetString(1);

                                dic.Add(name, value);
                            }
                        }
                    }
                }
            }

            return dic;
        }

        /// <summary>
        /// Get stopwords from the database
        /// </summary>
        /// <returns>
        /// collection of stopwords
        /// </returns>
        public override StringCollection LoadStopWords()
        {
            var col = new StringCollection();
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn != null)
                {
                    conn.ConnectionString = connString;

                    using (var cmd = conn.CreateCommand())
                    {
                        var sqlQuery = string.Format("SELECT StopWord FROM {0}StopWords", this.tablePrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;
                        conn.Open();

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                if (!col.Contains(rdr.GetString(0)))
                                {
                                    col.Add(rdr.GetString(0));
                                }
                            }
                        }
                    }
                }
            }

            return col;
        }

        /// <summary>
        /// Deletes an item from the dataStore
        /// </summary>
        /// <param name="extensionType">
        /// type of item
        /// </param>
        /// <param name="extensionId">
        /// id of item
        /// </param>
        public override void RemoveFromDataStore(ExtensionType extensionType, string extensionId)
        {
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("DELETE FROM {0}DataStoreSettings WHERE ExtensionType = {1}type AND ExtensionId = {2}id", this.tablePrefix, this.parmPrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var id = provider.CreateParameter();
                    if (id != null)
                    {
                        id.ParameterName = string.Format("{0}type", this.parmPrefix);
                        id.Value = extensionType;
                        cmd.Parameters.Add(id);
                    }

                    var type = provider.CreateParameter();
                    if (type != null)
                    {
                        type.ParameterName = string.Format("{0}id", this.parmPrefix);
                        type.Value = extensionId;
                        cmd.Parameters.Add(type);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Saves the PingServices to the database
        /// </summary>
        /// <param name="services">
        /// collection of PingServices
        /// </param>
        public override void SavePingServices(StringCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("DELETE FROM {0}PingService", this.tablePrefix);
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();

                    foreach (var service in services)
                    {
                        sqlQuery = string.Format("INSERT INTO {0}PingService (Link) VALUES ({1}link)", this.tablePrefix, this.parmPrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.Parameters.Clear();

                        var linkDp = provider.CreateParameter();
                        if (linkDp != null)
                        {
                            linkDp.ParameterName = string.Format("{0}link", this.parmPrefix);
                            linkDp.Value = service;
                            cmd.Parameters.Add(linkDp);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Saves the settings to the database
        /// </summary>
        /// <param name="settings">
        /// dictionary of settings
        /// </param>
        public override void SaveSettings(StringDictionary settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("DELETE FROM {0}Settings", this.tablePrefix);
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();

                    foreach (string key in settings.Keys)
                    {
                        sqlQuery = string.Format("INSERT INTO {0}Settings (SettingName, SettingValue) VALUES ({1}name, {2}value)", this.tablePrefix, this.parmPrefix, this.parmPrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.Parameters.Clear();

                        var nameDp = provider.CreateParameter();
                        if (nameDp != null)
                        {
                            nameDp.ParameterName = string.Format("{0}name", this.parmPrefix);
                            nameDp.Value = key;
                            cmd.Parameters.Add(nameDp);
                        }

                        var valueDp = provider.CreateParameter();
                        if (valueDp != null)
                        {
                            valueDp.ParameterName = string.Format("{0}value", this.parmPrefix);
                            valueDp.Value = settings[key];
                            cmd.Parameters.Add(valueDp);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Save to DataStore
        /// </summary>
        /// <param name="extensionType">
        /// type of info
        /// </param>
        /// <param name="extensionId">
        /// id of info
        /// </param>
        /// <param name="settings">
        /// data of info
        /// </param>
        public override void SaveToDataStore(ExtensionType extensionType, string extensionId, object settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            // Save
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            var xs = new XmlSerializer(settings.GetType());
            string objectXml;
            using (var sw = new StringWriter())
            {
                xs.Serialize(sw, settings);
                objectXml = sw.ToString();
            }

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("DELETE FROM {0}DataStoreSettings WHERE ExtensionType = @type AND ExtensionId = @id; ", this.tablePrefix);

                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var id = provider.CreateParameter();
                    if (id != null)
                    {
                        id.ParameterName = string.Format("{0}type", this.parmPrefix);
                        id.Value = extensionType.GetHashCode();
                        cmd.Parameters.Add(id);
                    }

                    var type = provider.CreateParameter();
                    if (type != null)
                    {
                        type.ParameterName = string.Format("{0}id", this.parmPrefix);
                        type.Value = extensionId;
                        cmd.Parameters.Add(type);
                    }

                    cmd.ExecuteNonQuery();

                    sqlQuery = string.Format("INSERT INTO {0}DataStoreSettings (ExtensionType, ExtensionId, Settings) VALUES (@type, @id, @file)", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var fileDp = provider.CreateParameter();
                    if (fileDp != null)
                    {
                        fileDp.ParameterName = string.Format("{0}file", this.parmPrefix);
                        fileDp.Value = objectXml; // settings.ToString(); // file;
                        cmd.Parameters.Add(fileDp);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
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
            var blogRoll = BlogRollItem.BlogRolls.Find(br => br.Id == id) ?? new BlogRollItem();

            blogRoll.MarkOld();
            return blogRoll;
        }

        /// <summary>
        /// Returns a category
        /// </summary>
        /// <param name="id">Id of category to return</param>
        /// <returns>A category.</returns>
        public override Category SelectCategory(Guid id)
        {
            var categories = Category.Categories;

            var category = new Category();

            foreach (var cat in categories.Where(cat => cat.Id == id))
            {
                category = cat;
            }

            category.MarkOld();
            return category;
        }

        /// <summary>
        /// Returns a page for given ID
        /// </summary>
        /// <param name="id">
        /// ID of page to return
        /// </param>
        /// <returns>
        /// selected page
        /// </returns>
        public override Page SelectPage(Guid id)
        {
            var page = new Page();

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn != null)
                {
                    conn.ConnectionString = connString;

                    using (var cmd = conn.CreateCommand())
                    {
                        var sqlQuery = string.Format("SELECT PageID, Title, Description, PageContent, DateCreated,    DateModified, Keywords, IsPublished, IsFrontPage, Parent, ShowInList, Slug FROM {0}Pages WHERE PageID = {1}id", this.tablePrefix, this.parmPrefix);

                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        var idparameter = provider.CreateParameter();
                        if (idparameter != null)
                        {
                            idparameter.ParameterName = string.Format("{0}id", this.parmPrefix);
                            idparameter.Value = id.ToString();
                            cmd.Parameters.Add(idparameter);
                        }

                        conn.Open();
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                rdr.Read();

                                page.Id = rdr.GetGuid(0);
                                page.Title = rdr.IsDBNull(1) ? String.Empty : rdr.GetString(1);
                                page.Content = rdr.IsDBNull(3) ? String.Empty : rdr.GetString(3);
                                page.Description = rdr.IsDBNull(2) ? String.Empty : rdr.GetString(2);
                                if (!rdr.IsDBNull(4))
                                {
                                    page.DateCreated = rdr.GetDateTime(4);
                                }

                                if (!rdr.IsDBNull(5))
                                {
                                    page.DateModified = rdr.GetDateTime(5);
                                }

                                if (!rdr.IsDBNull(6))
                                {
                                    page.Keywords = rdr.GetString(6);
                                }

                                if (!rdr.IsDBNull(7))
                                {
                                    page.Published = rdr.GetBoolean(7);
                                }

                                if (!rdr.IsDBNull(8))
                                {
                                    page.FrontPage = rdr.GetBoolean(8);
                                }

                                if (!rdr.IsDBNull(9))
                                {
                                    page.Parent = rdr.GetGuid(9);
                                }

                                if (!rdr.IsDBNull(10))
                                {
                                    page.ShowInList = rdr.GetBoolean(10);
                                }

                                if (!rdr.IsDBNull(11))
                                {
                                    page.Slug = rdr.GetString(11);
                                }
                            }
                        }
                    }
                }
            }

            return page;
        }

        /// <summary>
        /// Returns a Post based on Id.
        /// </summary>
        /// <param name="id">
        /// The Post ID.
        /// </param>
        /// <returns>
        /// The Post..
        /// </returns>
        public override Post SelectPost(Guid id)
        {
            var post = new Post();
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return post;
                }

                conn.ConnectionString = connString;

                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("SELECT PostID, Title, Description, PostContent, DateCreated, DateModified, Author, IsPublished, IsCommentEnabled, Raters, Rating, Slug FROM {0}Posts WHERE PostID = {1}id", this.tablePrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var idparameter = provider.CreateParameter();
                    if (idparameter != null)
                    {
                        idparameter.ParameterName = string.Format("{0}id", this.parmPrefix);
                        idparameter.Value = id.ToString();
                        cmd.Parameters.Add(idparameter);
                    }

                    conn.Open();

                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            rdr.Read();

                            post.Id = rdr.GetGuid(0);
                            post.Title = rdr.GetString(1);
                            post.Content = rdr.GetString(3);
                            post.Description = rdr.IsDBNull(2) ? String.Empty : rdr.GetString(2);
                            if (!rdr.IsDBNull(4))
                            {
                                post.DateCreated = rdr.GetDateTime(4);
                            }

                            if (!rdr.IsDBNull(5))
                            {
                                post.DateModified = rdr.GetDateTime(5);
                            }

                            if (!rdr.IsDBNull(6))
                            {
                                post.Author = rdr.GetString(6);
                            }

                            if (!rdr.IsDBNull(7))
                            {
                                post.Published = rdr.GetBoolean(7);
                            }

                            if (!rdr.IsDBNull(8))
                            {
                                post.HasCommentsEnabled = rdr.GetBoolean(8);
                            }

                            if (!rdr.IsDBNull(9))
                            {
                                post.Raters = rdr.GetInt32(9);
                            }

                            if (!rdr.IsDBNull(10))
                            {
                                post.Rating = rdr.GetFloat(10);
                            }

                            post.Slug = !rdr.IsDBNull(11) ? rdr.GetString(11) : string.Empty;
                        }
                    }

                    // Tags
                    sqlQuery = string.Format("SELECT Tag FROM {0}PostTag WHERE PostID = {1}id", this.tablePrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(0))
                            {
                                post.Tags.Add(rdr.GetString(0));
                            }
                        }
                    }

                    post.Tags.MarkOld();

                    // Categories
                    sqlQuery = string.Format("SELECT CategoryID FROM {0}PostCategory WHERE PostID = {1}id", this.tablePrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var key = rdr.GetGuid(0);
                            if (Category.GetCategory(key) != null)
                            {
                                post.Categories.Add(Category.GetCategory(key));
                            }
                        }
                    }

                    // Comments
                    sqlQuery =
                        string.Format("SELECT PostCommentID, CommentDate, Author, Email, Website, Comment, Country, Ip, IsApproved, ParentCommentID, ModeratedBy, Avatar, IsSpam, IsDeleted FROM {0}PostComment WHERE PostID = {1}id", this.tablePrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var comment = new Comment
                                {
                                    Id = rdr.GetGuid(0),
                                    IsApproved = true,
                                    Author = rdr.GetString(2)
                                };
                            if (!rdr.IsDBNull(4))
                            {
                                Uri website;
                                if (Uri.TryCreate(rdr.GetString(4), UriKind.Absolute, out website))
                                {
                                    comment.Website = website;
                                }
                            }

                            comment.Email = rdr.GetString(3);
                            comment.Content = rdr.GetString(5);
                            comment.DateCreated = rdr.GetDateTime(1);
                            comment.Parent = post;

                            if (!rdr.IsDBNull(6))
                            {
                                comment.Country = rdr.GetString(6);
                            }

                            if (!rdr.IsDBNull(7))
                            {
                                comment.IP = rdr.GetString(7);
                            }

                            comment.IsApproved = rdr.IsDBNull(8) || rdr.GetBoolean(8);

                            comment.ParentId = rdr.GetGuid(9);

                            if (!rdr.IsDBNull(10))
                            {
                                comment.ModeratedBy = rdr.GetString(10);
                            }

                            if (!rdr.IsDBNull(11))
                            {
                                comment.Avatar = rdr.GetString(11);
                            }

                            if (!rdr.IsDBNull(12))
                            {
                                comment.IsSpam = rdr.GetBoolean(12);
                            }

                            if (!rdr.IsDBNull(13))
                            {
                                comment.IsDeleted = rdr.GetBoolean(13);
                            }

                            post.Comments.Add(comment);
                        }
                    }

                    post.Comments.Sort();

                    // Email Notification
                    sqlQuery = string.Format("SELECT NotifyAddress FROM {0}PostNotify WHERE PostID = {1}id", this.tablePrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(0))
                            {
                                post.NotificationEmails.Add(rdr.GetString(0));
                            }
                        }
                    }
                }
            }

            return post;
        }

        /// <summary>
        /// Loads AuthorProfile from database
        /// </summary>
        /// <param name="id">The user name.</param>
        /// <returns>An AuthorProfile.</returns>
        public override AuthorProfile SelectProfile(string id)
        {
            var dic = new StringDictionary();
            var profile = new AuthorProfile(id);

            // Retrieve Profile data from Db
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn != null)
                {
                    conn.ConnectionString = connString;
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        var sqlQuery = string.Format("SELECT SettingName, SettingValue FROM {0}Profiles WHERE UserName = {1}name", this.tablePrefix, this.parmPrefix);
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        var nameDp = provider.CreateParameter();
                        if (nameDp != null)
                        {
                            nameDp.ParameterName = string.Format("{0}name", this.parmPrefix);
                            nameDp.Value = id;
                            cmd.Parameters.Add(nameDp);
                        }

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                dic.Add(rdr.GetString(0), rdr.GetString(1));
                            }
                        }
                    }
                }
            }

            // Load profile with data from dictionary
            if (dic.ContainsKey("DisplayName"))
            {
                profile.DisplayName = dic["DisplayName"];
            }

            if (dic.ContainsKey("FirstName"))
            {
                profile.FirstName = dic["FirstName"];
            }

            if (dic.ContainsKey("MiddleName"))
            {
                profile.MiddleName = dic["MiddleName"];
            }

            if (dic.ContainsKey("LastName"))
            {
                profile.LastName = dic["LastName"];
            }

            if (dic.ContainsKey("CityTown"))
            {
                profile.CityTown = dic["CityTown"];
            }

            if (dic.ContainsKey("RegionState"))
            {
                profile.RegionState = dic["RegionState"];
            }

            if (dic.ContainsKey("Country"))
            {
                profile.Country = dic["Country"];
            }

            if (dic.ContainsKey("Birthday"))
            {
                DateTime date;
                if (DateTime.TryParse(dic["Birthday"], out date))
                {
                    profile.Birthday = date;
                }
            }

            if (dic.ContainsKey("AboutMe"))
            {
                profile.AboutMe = dic["AboutMe"];
            }

            if (dic.ContainsKey("PhotoURL"))
            {
                profile.PhotoUrl = dic["PhotoURL"];
            }

            if (dic.ContainsKey("Company"))
            {
                profile.Company = dic["Company"];
            }

            if (dic.ContainsKey("EmailAddress"))
            {
                profile.EmailAddress = dic["EmailAddress"];
            }

            if (dic.ContainsKey("PhoneMain"))
            {
                profile.PhoneMain = dic["PhoneMain"];
            }

            if (dic.ContainsKey("PhoneMobile"))
            {
                profile.PhoneMobile = dic["PhoneMobile"];
            }

            if (dic.ContainsKey("PhoneFax"))
            {
                profile.PhoneFax = dic["PhoneFax"];
            }

            if (dic.ContainsKey("IsPrivate"))
            {
                profile.Private = dic["IsPrivate"] == "true";
            }

            return profile;
        }

        /// <summary>
        /// Gets a Referrer based on an Id.
        /// </summary>
        /// <param name="id">
        /// The Referrer Id.
        /// </param>
        /// <returns>
        /// A matching Referrer
        /// </returns>
        public override Referrer SelectReferrer(Guid id)
        {
            var refer = Referrer.Referrers.Find(r => r.Id.Equals(id)) ?? new Referrer();

            refer.MarkOld();
            return refer;
        }

        /// <summary>
        /// Storage location on web server
        /// </summary>
        /// <returns>
        /// string with virtual path to storage
        /// </returns>
        public override string StorageLocation()
        {
            return String.IsNullOrEmpty(WebConfigurationManager.AppSettings["StorageLocation"])
                       ? @"~/app_data/"
                       : WebConfigurationManager.AppSettings["StorageLocation"];
        }

        /// <summary>
        /// Saves an existing BlogRoll to the database
        /// </summary>
        /// <param name="blogRollItem">
        /// BlogRoll to be saved
        /// </param>
        public override void UpdateBlogRollItem(BlogRollItem blogRollItem)
        {
            var blogRolls = BlogRollItem.BlogRolls;
            blogRolls.Remove(blogRollItem);
            blogRolls.Add(blogRollItem);

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("UPDATE {0}BlogRollItems SET Title = @Title, Description = @Description, BlogUrl = @BlogUrl, FeedUrl = @FeedUrl, Xfn = @Xfn, SortIndex = @SortIndex WHERE BlogRollId = @BlogRollId", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    this.AddBlogRollParametersToCommand(blogRollItem, provider, cmd);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Saves an existing category to the database
        /// </summary>
        /// <param name="category">
        /// category to be saved
        /// </param>
        public override void UpdateCategory(Category category)
        {
            var categories = Category.Categories;
            categories.Remove(category);
            categories.Add(category);

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("UPDATE {0}Categories SET CategoryName = @catname, Description = @description, ParentID = @parentid WHERE CategoryID = @catid", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var catid = provider.CreateParameter();
                    if (catid != null)
                    {
                        catid.ParameterName = string.Format("{0}catid", this.parmPrefix);
                        catid.Value = category.Id.ToString();
                        cmd.Parameters.Add(catid);
                    }

                    var title = provider.CreateParameter();
                    if (title != null)
                    {
                        title.ParameterName = string.Format("{0}catname", this.parmPrefix);
                        title.Value = category.Title;
                        cmd.Parameters.Add(title);
                    }

                    var desc = provider.CreateParameter();
                    if (desc != null)
                    {
                        desc.ParameterName = string.Format("{0}description", this.parmPrefix);
                        desc.Value = category.Description;
                        cmd.Parameters.Add(desc);
                    }

                    var parent = provider.CreateParameter();
                    if (parent != null)
                    {
                        parent.ParameterName = string.Format("{0}parentid", this.parmPrefix);
                        parent.Value = category.Parent == null ? (object)DBNull.Value : category.Parent.ToString();

                        cmd.Parameters.Add(parent);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Saves an existing page in the database
        /// </summary>
        /// <param name="page">
        /// page to be saved
        /// </param>
        public override void UpdatePage(Page page)
        {
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("UPDATE {0}Pages SET Title = @title, Description = @desc, PageContent = @content, DateCreated = @created, DateModified = @modified, Keywords = @keywords, IsPublished = @ispublished, IsFrontPage = @isfrontpage, Parent = @parent, ShowInList = @showinlist, Slug = @slug WHERE PageID = @id", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var idparameter = provider.CreateParameter();
                    if (idparameter != null)
                    {
                        idparameter.ParameterName = string.Format("{0}id", this.parmPrefix);
                        idparameter.Value = page.Id.ToString();
                        cmd.Parameters.Add(idparameter);
                    }

                    var title = provider.CreateParameter();
                    if (title != null)
                    {
                        title.ParameterName = string.Format("{0}title", this.parmPrefix);
                        title.Value = page.Title;
                        cmd.Parameters.Add(title);
                    }

                    var desc = provider.CreateParameter();
                    if (desc != null)
                    {
                        desc.ParameterName = string.Format("{0}desc", this.parmPrefix);
                        desc.Value = page.Description;
                        cmd.Parameters.Add(desc);
                    }

                    var content = provider.CreateParameter();
                    if (content != null)
                    {
                        content.ParameterName = string.Format("{0}content", this.parmPrefix);
                        content.Value = page.Content;
                        cmd.Parameters.Add(content);
                    }

                    var created = provider.CreateParameter();
                    if (created != null)
                    {
                        created.ParameterName = string.Format("{0}created", this.parmPrefix);
                        created.Value = page.DateCreated.AddHours(-BlogSettings.Instance.Timezone);
                        cmd.Parameters.Add(created);
                    }

                    var modified = provider.CreateParameter();
                    if (modified != null)
                    {
                        modified.ParameterName = string.Format("{0}modified", this.parmPrefix);
                        modified.Value = page.DateModified == new DateTime() ? DateTime.Now : page.DateModified.AddHours(-BlogSettings.Instance.Timezone);
                        cmd.Parameters.Add(modified);
                    }

                    var keywords = provider.CreateParameter();
                    if (keywords != null)
                    {
                        keywords.ParameterName = string.Format("{0}keywords", this.parmPrefix);
                        keywords.Value = page.Keywords;
                        cmd.Parameters.Add(keywords);
                    }

                    var published = provider.CreateParameter();
                    if (published != null)
                    {
                        published.ParameterName = string.Format("{0}ispublished", this.parmPrefix);
                        published.Value = page.Published;
                        cmd.Parameters.Add(published);
                    }

                    var frontPage = provider.CreateParameter();
                    if (frontPage != null)
                    {
                        frontPage.ParameterName = string.Format("{0}isfrontpage", this.parmPrefix);
                        frontPage.Value = page.FrontPage;
                        cmd.Parameters.Add(frontPage);
                    }

                    var parent = provider.CreateParameter();
                    if (parent != null)
                    {
                        parent.ParameterName = string.Format("{0}parent", this.parmPrefix);
                        parent.Value = page.Parent.ToString();
                        cmd.Parameters.Add(parent);
                    }

                    var showInList = provider.CreateParameter();
                    if (showInList != null)
                    {
                        showInList.ParameterName = string.Format("{0}showinlist", this.parmPrefix);
                        showInList.Value = page.ShowInList;
                        cmd.Parameters.Add(showInList);
                    }

                    var slug = provider.CreateParameter();
                    if (slug != null)
                    {
                        slug.ParameterName = string.Format("{0}slug", this.parmPrefix);
                        slug.Value = page.Slug;
                        cmd.Parameters.Add(slug);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Saves and existing post in the database
        /// </summary>
        /// <param name="post">
        /// post to be saved
        /// </param>
        public override void UpdatePost(Post post)
        {
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("UPDATE {0}Posts SET Title = @title, Description = @desc, PostContent = @content, DateCreated = @created, DateModified = @modified, Author = @Author, IsPublished = @published, IsCommentEnabled = @commentEnabled, Raters = @raters, Rating = @rating, Slug = @slug WHERE PostID = @id", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var dpID = provider.CreateParameter();
                    dpID.ParameterName = this.parmPrefix + "id";
                    dpID.Value = post.Id.ToString();
                    cmd.Parameters.Add(dpID);

                    var dpTitle = provider.CreateParameter();
                    dpTitle.ParameterName = this.parmPrefix + "title";
                    dpTitle.Value = post.Title;
                    cmd.Parameters.Add(dpTitle);

                    var dpDesc = provider.CreateParameter();
                    dpDesc.ParameterName = this.parmPrefix + "desc";
                    dpDesc.Value = post.Description ?? string.Empty;
                    cmd.Parameters.Add(dpDesc);

                    var dpContent = provider.CreateParameter();
                    dpContent.ParameterName = this.parmPrefix + "content";
                    dpContent.Value = post.Content;
                    cmd.Parameters.Add(dpContent);

                    var dpCreated = provider.CreateParameter();
                    dpCreated.ParameterName = this.parmPrefix + "created";
                    dpCreated.Value = post.DateCreated.AddHours(-BlogSettings.Instance.Timezone);
                    cmd.Parameters.Add(dpCreated);

                    var dpModified = provider.CreateParameter();
                    dpModified.ParameterName = this.parmPrefix + "modified";
                    dpModified.Value = post.DateModified == new DateTime() ? DateTime.Now : post.DateModified.AddHours(-BlogSettings.Instance.Timezone);
                    cmd.Parameters.Add(dpModified);

                    var dpAuthor = provider.CreateParameter();
                    dpAuthor.ParameterName = this.parmPrefix + "author";
                    dpAuthor.Value = post.Author ?? string.Empty;
                    cmd.Parameters.Add(dpAuthor);

                    var dpPublished = provider.CreateParameter();
                    dpPublished.ParameterName = this.parmPrefix + "published";
                    dpPublished.Value = post.Published;
                    cmd.Parameters.Add(dpPublished);

                    var dpCommentEnabled = provider.CreateParameter();
                    dpCommentEnabled.ParameterName = this.parmPrefix + "commentEnabled";
                    dpCommentEnabled.Value = post.HasCommentsEnabled;
                    cmd.Parameters.Add(dpCommentEnabled);

                    var dpRaters = provider.CreateParameter();
                    dpRaters.ParameterName = this.parmPrefix + "raters";
                    dpRaters.Value = post.Raters;
                    cmd.Parameters.Add(dpRaters);

                    var dpRating = provider.CreateParameter();
                    dpRating.ParameterName = this.parmPrefix + "rating";
                    dpRating.Value = post.Rating;
                    cmd.Parameters.Add(dpRating);

                    var dpSlug = provider.CreateParameter();
                    dpSlug.ParameterName = this.parmPrefix + "slug";
                    dpSlug.Value = post.Slug ?? string.Empty;
                    cmd.Parameters.Add(dpSlug);

                    cmd.ExecuteNonQuery();
                }

                // Tags
                this.UpdateTags(post, conn, provider);

                // Categories
                this.UpdateCategories(post, conn, provider);

                // Comments
                this.UpdateComments(post, conn, provider);

                // Email Notification
                this.UpdateNotify(post, conn, provider);
            }
        }

        /// <summary>
        /// Updates AuthorProfile to database
        /// </summary>
        /// <param name="profile">
        /// An AuthorProfile.
        /// </param>
        public override void UpdateProfile(AuthorProfile profile)
        {
            // Remove Profile
            this.DeleteProfile(profile);

            // Create Profile Dictionary
            var dic = new StringDictionary();

            if (!String.IsNullOrEmpty(profile.DisplayName))
            {
                dic.Add("DisplayName", profile.DisplayName);
            }

            if (!String.IsNullOrEmpty(profile.FirstName))
            {
                dic.Add("FirstName", profile.FirstName);
            }

            if (!String.IsNullOrEmpty(profile.MiddleName))
            {
                dic.Add("MiddleName", profile.MiddleName);
            }

            if (!String.IsNullOrEmpty(profile.LastName))
            {
                dic.Add("LastName", profile.LastName);
            }

            if (!String.IsNullOrEmpty(profile.CityTown))
            {
                dic.Add("CityTown", profile.CityTown);
            }

            if (!String.IsNullOrEmpty(profile.RegionState))
            {
                dic.Add("RegionState", profile.RegionState);
            }

            if (!String.IsNullOrEmpty(profile.Country))
            {
                dic.Add("Country", profile.Country);
            }

            if (!String.IsNullOrEmpty(profile.AboutMe))
            {
                dic.Add("AboutMe", profile.AboutMe);
            }

            if (!String.IsNullOrEmpty(profile.PhotoUrl))
            {
                dic.Add("PhotoURL", profile.PhotoUrl);
            }

            if (!String.IsNullOrEmpty(profile.Company))
            {
                dic.Add("Company", profile.Company);
            }

            if (!String.IsNullOrEmpty(profile.EmailAddress))
            {
                dic.Add("EmailAddress", profile.EmailAddress);
            }

            if (!String.IsNullOrEmpty(profile.PhoneMain))
            {
                dic.Add("PhoneMain", profile.PhoneMain);
            }

            if (!String.IsNullOrEmpty(profile.PhoneMobile))
            {
                dic.Add("PhoneMobile", profile.PhoneMobile);
            }

            if (!String.IsNullOrEmpty(profile.PhoneFax))
            {
                dic.Add("PhoneFax", profile.PhoneFax);
            }

            if (profile.Birthday != DateTime.MinValue)
            {
                dic.Add("Birthday", profile.Birthday.ToString("yyyy-MM-dd"));
            }

            dic.Add("IsPrivate", profile.Private.ToString());

            // Save Profile Dictionary
            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                conn.ConnectionString = connString;
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    foreach (string key in dic.Keys)
                    {
                        var sqlQuery = string.Format("INSERT INTO {0}Profiles (UserName, SettingName, SettingValue) VALUES (@user, @name, @value)", this.tablePrefix);
                        if (this.parmPrefix != "@")
                        {
                            sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                        }

                        cmd.CommandText = sqlQuery;
                        cmd.Parameters.Clear();

                        var dpUser = provider.CreateParameter();
                        dpUser.ParameterName = this.parmPrefix + "user";
                        dpUser.Value = profile.Id;
                        cmd.Parameters.Add(dpUser);

                        var dpName = provider.CreateParameter();
                        dpName.ParameterName = this.parmPrefix + "name";
                        dpName.Value = key;
                        cmd.Parameters.Add(dpName);

                        var dpValue = provider.CreateParameter();
                        dpValue.ParameterName = this.parmPrefix + "value";
                        dpValue.Value = dic[key];
                        cmd.Parameters.Add(dpValue);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Saves an existing Referrer to the database.
        /// </summary>
        /// <param name="referrer">
        /// Referrer to be saved.
        /// </param>
        public override void UpdateReferrer(Referrer referrer)
        {
            var referrers = Referrer.Referrers;
            referrers.Remove(referrer);
            referrers.Add(referrer);

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("UPDATE {0}Referrers SET ReferralDay = @ReferralDay, ReferrerUrl = @ReferrerUrl, ReferralCount = @ReferralCount, Url = @Url, IsSpam = @IsSpam WHERE ReferrerId = @ReferrerId", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    this.AddReferrersParametersToCommand(referrer, provider, cmd);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The update categories.
        /// </summary>
        /// <param name="post">
        /// The post to update.
        /// </param>
        /// <param name="conn">
        /// The connection.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        private void UpdateCategories(Post post, DbConnection conn, DbProviderFactory provider)
        {
            var sqlQuery = string.Format("DELETE FROM {0}PostCategory WHERE PostID = {1}id", this.tablePrefix, this.parmPrefix);
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                cmd.CommandType = CommandType.Text;
                var id = provider.CreateParameter();
                if (id != null)
                {
                    id.ParameterName = string.Format("{0}id", this.parmPrefix);
                    id.Value = post.Id.ToString();
                    cmd.Parameters.Add(id);
                }

                cmd.ExecuteNonQuery();

                foreach (var cat in post.Categories)
                {
                    cmd.CommandText = string.Format("INSERT INTO {0}PostCategory (PostID, CategoryID) VALUES ({1}id, {2}cat)", this.tablePrefix, this.parmPrefix, this.parmPrefix);
                    cmd.Parameters.Clear();
                    var postId = provider.CreateParameter();
                    if (postId != null)
                    {
                        postId.ParameterName = string.Format("{0}id", this.parmPrefix);
                        postId.Value = post.Id.ToString();
                        cmd.Parameters.Add(postId);
                    }

                    var category = provider.CreateParameter();
                    if (category != null)
                    {
                        category.ParameterName = string.Format("{0}cat", this.parmPrefix);
                        category.Value = cat.Id.ToString();
                        cmd.Parameters.Add(category);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// The update comments.
        /// </summary>
        /// <param name="post">
        /// The post to update.
        /// </param>
        /// <param name="conn">
        /// The connection.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        private void UpdateComments(Post post, DbConnection conn, DbProviderFactory provider)
        {
            var sqlQuery = string.Format("DELETE FROM {0}PostComment WHERE PostID = {1}id", this.tablePrefix, this.parmPrefix);
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                cmd.CommandType = CommandType.Text;
                var id = provider.CreateParameter();
                if (id != null)
                {
                    id.ParameterName = string.Format("{0}id", this.parmPrefix);
                    id.Value = post.Id.ToString();
                    cmd.Parameters.Add(id);
                }

                cmd.ExecuteNonQuery();

                foreach (var comment in post.Comments)
                {
                    sqlQuery = string.Format("INSERT INTO {0}PostComment (PostCommentID, ParentCommentID, PostID, CommentDate, Author, Email, Website, Comment, Country, Ip, IsApproved, ModeratedBy, Avatar, IsSpam, IsDeleted) VALUES (@postcommentid, @parentid, @id, @date, @author, @email, @website, @comment, @country, @ip, @isapproved, @moderatedby, @avatar, @isspam, @isdeleted)", this.tablePrefix);
                    if (this.parmPrefix != "@")
                    {
                        sqlQuery = sqlQuery.Replace("@", this.parmPrefix);
                    }

                    cmd.CommandText = sqlQuery;
                    cmd.Parameters.Clear();
                    var commentId = provider.CreateParameter();
                    if (commentId != null)
                    {
                        commentId.ParameterName = string.Format("{0}postcommentid", this.parmPrefix);
                        commentId.Value = comment.Id.ToString();
                        cmd.Parameters.Add(commentId);
                    }

                    var parentId = provider.CreateParameter();
                    if (parentId != null)
                    {
                        parentId.ParameterName = string.Format("{0}parentid", this.parmPrefix);
                        parentId.Value = comment.ParentId.ToString();
                        cmd.Parameters.Add(parentId);
                    }

                    var postId = provider.CreateParameter();
                    if (postId != null)
                    {
                        postId.ParameterName = string.Format("{0}id", this.parmPrefix);
                        postId.Value = post.Id.ToString();
                        cmd.Parameters.Add(postId);
                    }

                    var dpCommentDate = provider.CreateParameter();
                    dpCommentDate.ParameterName = this.parmPrefix + "date";
                    dpCommentDate.Value = comment.DateCreated.AddHours(-BlogSettings.Instance.Timezone);
                    cmd.Parameters.Add(dpCommentDate);

                    var dpAuthor = provider.CreateParameter();
                    dpAuthor.ParameterName = this.parmPrefix + "author";
                    dpAuthor.Value = comment.Author;
                    cmd.Parameters.Add(dpAuthor);

                    var dpEmail = provider.CreateParameter();
                    dpEmail.ParameterName = this.parmPrefix + "email";
                    dpEmail.Value = comment.Email ?? string.Empty;
                    cmd.Parameters.Add(dpEmail);

                    var dpWebsite = provider.CreateParameter();
                    if (dpWebsite != null)
                    {
                        dpWebsite.ParameterName = string.Format("{0}website", this.parmPrefix);
                        dpWebsite.Value = comment.Website == null ? string.Empty : comment.Website.ToString();
                        cmd.Parameters.Add(dpWebsite);
                    }

                    var dpContent = provider.CreateParameter();
                    dpContent.ParameterName = this.parmPrefix + "comment";
                    dpContent.Value = comment.Content;
                    cmd.Parameters.Add(dpContent);

                    var dpCountry = provider.CreateParameter();
                    dpCountry.ParameterName = this.parmPrefix + "country";
                    dpCountry.Value = comment.Country ?? string.Empty;
                    cmd.Parameters.Add(dpCountry);

                    var dpIP = provider.CreateParameter();
                    dpIP.ParameterName = this.parmPrefix + "ip";
                    dpIP.Value = comment.IP ?? string.Empty;
                    cmd.Parameters.Add(dpIP);

                    var dpIsApproved = provider.CreateParameter();
                    dpIsApproved.ParameterName = this.parmPrefix + "isapproved";
                    dpIsApproved.Value = comment.IsApproved;
                    cmd.Parameters.Add(dpIsApproved);

                    var dpModeratedBy = provider.CreateParameter();
                    dpModeratedBy.ParameterName = this.parmPrefix + "moderatedby";
                    dpModeratedBy.Value = comment.ModeratedBy ?? string.Empty;
                    cmd.Parameters.Add(dpModeratedBy);

                    var dpAvatar = provider.CreateParameter();
                    dpAvatar.ParameterName = this.parmPrefix + "avatar";
                    dpAvatar.Value = comment.Avatar ?? string.Empty;
                    cmd.Parameters.Add(dpAvatar);

                    var dpIsSpam = provider.CreateParameter();
                    dpIsSpam.ParameterName = this.parmPrefix + "isspam";
                    dpIsSpam.Value = comment.IsSpam;
                    cmd.Parameters.Add(dpIsSpam);

                    var dpIsDeleted = provider.CreateParameter();
                    dpIsDeleted.ParameterName = this.parmPrefix + "isdeleted";
                    dpIsDeleted.Value = comment.IsDeleted;
                    cmd.Parameters.Add(dpIsDeleted);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// The update notify.
        /// </summary>
        /// <param name="post">
        /// The post to update.
        /// </param>
        /// <param name="conn">
        /// The connection.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        private void UpdateNotify(Post post, DbConnection conn, DbProviderFactory provider)
        {
            var sqlQuery = string.Format("DELETE FROM {0}PostNotify WHERE PostID = {1}id", this.tablePrefix, this.parmPrefix);
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                cmd.CommandType = CommandType.Text;
                var id = provider.CreateParameter();
                if (id != null)
                {
                    id.ParameterName = string.Format("{0}id", this.parmPrefix);
                    id.Value = post.Id.ToString();
                    cmd.Parameters.Add(id);
                }

                cmd.ExecuteNonQuery();

                foreach (var email in post.NotificationEmails)
                {
                    cmd.CommandText = string.Format("INSERT INTO {0}PostNotify (PostID, NotifyAddress) VALUES ({1}id, {2}notify)", this.tablePrefix, this.parmPrefix, this.parmPrefix);
                    cmd.Parameters.Clear();
                    var postId = provider.CreateParameter();
                    if (postId != null)
                    {
                        postId.ParameterName = string.Format("{0}id", this.parmPrefix);
                        postId.Value = post.Id.ToString();
                        cmd.Parameters.Add(postId);
                    }
                    var notify = provider.CreateParameter();
                    if (notify != null)
                    {
                        notify.ParameterName = string.Format("{0}notify", this.parmPrefix);
                        notify.Value = email;
                        cmd.Parameters.Add(notify);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// The update tags.
        /// </summary>
        /// <param name="post">
        /// The post to update.
        /// </param>
        /// <param name="conn">
        /// The connection.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        private void UpdateTags(Post post, DbConnection conn, DbProviderFactory provider)
        {
            var sqlQuery = string.Format("DELETE FROM {0}PostTag WHERE PostID = {1}id", this.tablePrefix, this.parmPrefix);
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                cmd.CommandType = CommandType.Text;
                var id = provider.CreateParameter();
                if (id != null)
                {
                    id.ParameterName = string.Format("{0}id", this.parmPrefix);
                    id.Value = post.Id.ToString();
                    cmd.Parameters.Add(id);
                }

                cmd.ExecuteNonQuery();

                foreach (var tag in post.Tags)
                {
                    cmd.CommandText = string.Format("INSERT INTO {0}PostTag (PostID, Tag) VALUES ({1}id, {2}tag)", this.tablePrefix, this.parmPrefix, this.parmPrefix);
                    cmd.Parameters.Clear();
                    var postId = provider.CreateParameter();
                    if (postId != null)
                    {
                        postId.ParameterName = string.Format("{0}id", this.parmPrefix);
                        postId.Value = post.Id.ToString();
                        cmd.Parameters.Add(postId);
                    }

                    var tag2 = provider.CreateParameter();
                    if (tag2 != null)
                    {
                        tag2.ParameterName = string.Format("{0}tag", this.parmPrefix);
                        tag2.Value = tag;
                        cmd.Parameters.Add(tag2);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// The add blog roll parameters to command.
        /// </summary>
        /// <param name="blogRollItem">
        /// The blog roll item.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="cmd">
        /// The command.
        /// </param>
        private void AddBlogRollParametersToCommand(
            BlogRollItem blogRollItem, DbProviderFactory provider, DbCommand cmd)
        {
            var dpID = provider.CreateParameter();
            dpID.ParameterName = this.parmPrefix + "BlogRollId";
            dpID.Value = blogRollItem.Id.ToString();
            cmd.Parameters.Add(dpID);

            var dpTitle = provider.CreateParameter();
            dpTitle.ParameterName = this.parmPrefix + "Title";
            dpTitle.Value = blogRollItem.Title;
            cmd.Parameters.Add(dpTitle);

            var dpDesc = provider.CreateParameter();
            dpDesc.ParameterName = this.parmPrefix + "Description";
            dpDesc.Value = blogRollItem.Description;
            cmd.Parameters.Add(dpDesc);

            var dpBlogUrl = provider.CreateParameter();
            dpBlogUrl.ParameterName = "BlogUrl";
            dpBlogUrl.Value = blogRollItem.BlogUrl != null ? (object)blogRollItem.BlogUrl.ToString() : DBNull.Value;
            cmd.Parameters.Add(dpBlogUrl);

            var dpFeedUrl = provider.CreateParameter();
            dpFeedUrl.ParameterName = "FeedUrl";
            dpFeedUrl.Value = blogRollItem.FeedUrl != null ? (object)blogRollItem.FeedUrl.ToString() : DBNull.Value;
            cmd.Parameters.Add(dpFeedUrl);

            var dpXfn = provider.CreateParameter();
            dpXfn.ParameterName = "Xfn";
            dpXfn.Value = blogRollItem.Xfn;
            cmd.Parameters.Add(dpXfn);

            var dpSortIndex = provider.CreateParameter();
            dpSortIndex.ParameterName = "SortIndex";
            dpSortIndex.Value = blogRollItem.SortIndex;
            cmd.Parameters.Add(dpSortIndex);
        }

        /// <summary>
        /// The add referrers parameters to command.
        /// </summary>
        /// <param name="referrer">
        /// The referrer.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="cmd">
        /// The command.
        /// </param>
        private void AddReferrersParametersToCommand(Referrer referrer, DbProviderFactory provider, DbCommand cmd)
        {
            var dpId = provider.CreateParameter();
            dpId.ParameterName = "ReferrerId";
            dpId.Value = referrer.Id.ToString();
            cmd.Parameters.Add(dpId);

            var dpDay = provider.CreateParameter();
            dpDay.ParameterName = this.parmPrefix + "ReferralDay";
            dpDay.Value = referrer.Day;
            cmd.Parameters.Add(dpDay);

            var dpReferrer = provider.CreateParameter();
            dpReferrer.ParameterName = this.parmPrefix + "ReferrerUrl";
            dpReferrer.Value = referrer.ReferrerUrl != null ? (object)referrer.ReferrerUrl.ToString() : DBNull.Value;
            cmd.Parameters.Add(dpReferrer);

            var dpCount = provider.CreateParameter();
            dpCount.ParameterName = this.parmPrefix + "ReferralCount";
            dpCount.Value = referrer.Count;
            cmd.Parameters.Add(dpCount);

            var dpUrl = provider.CreateParameter();
            dpUrl.ParameterName = "Url";
            dpUrl.Value = referrer.Url != null ? (object)referrer.Url.ToString() : DBNull.Value;
            cmd.Parameters.Add(dpUrl);

            var dpIsSpam = provider.CreateParameter();
            dpIsSpam.ParameterName = "IsSpam";
            dpIsSpam.Value = referrer.PossibleSpam;
            cmd.Parameters.Add(dpIsSpam);
        }

        /// <summary>
        /// The delete old referrers.
        /// </summary>
        private void DeleteOldReferrers()
        {
            var cutoff = DateTime.Today.AddDays(-BlogSettings.Instance.NumberOfReferrerDays);

            var connString = ConfigurationManager.ConnectionStrings[this.connStringName].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings[this.connStringName].ProviderName;
            var provider = DbProviderFactories.GetFactory(providerName);

            using (var conn = provider.CreateConnection())
            {
                if (conn == null)
                {
                    return;
                }

                conn.ConnectionString = connString;
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sqlQuery = string.Format("DELETE FROM {0}Referrers WHERE ReferralDay < {1}ReferralDay", this.tablePrefix, this.parmPrefix);
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    var theDay = provider.CreateParameter();
                    if (theDay != null)
                    {
                        theDay.ParameterName = string.Format("{0}ReferralDay", this.parmPrefix);
                        theDay.Value = cutoff;
                        cmd.Parameters.Add(theDay);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}