using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using blogEngine.Core.Providers;

namespace blogEngine.Core.Models
{


    /// <summary>
    /// Categories are a way to organize posts. 
    ///     A post can be in multiple categories.
    /// </summary>
    [Serializable]
    public class Category : BaseEntity<Guid>, IComparable<Category> // For category.Sort
    {
        #region Constants and Fields
        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        ///     The description.
        /// </summary>
        private string description;

        /// <summary>
        ///     The parent.
        /// </summary>
        private Guid? parent;

        /// <summary>
        ///     The title.
        /// </summary>
        private string title;

        private static List<Category> categories;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="Category"/> class. 
        /// </summary>
        static Category()
        {
            Folder = HttpContext.Current.Server.MapPath(BlogSettings.Instance.StorageLocation);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "Category" /> class.
        /// </summary>
        public Category()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Category"/> class.
        ///     The category.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        public Category(string title, string description)
        {
            this.Id = Guid.NewGuid();
            this.title = title;
            this.description = description;
            this.Parent = null;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the absolute link to the page displaying all posts for this category.
        /// </summary>
        /// <value>The absolute link.</value>
        public Uri AbsoluteLink
        {
            get
            {
                return Utils.ConvertToAbsolute(this.RelativeLink);
            }
        }

        /// <summary>
        ///     Gets or sets the Description of the object.
        /// </summary>
        /// <value>The description.</value>
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
        ///     Gets the absolute link to the feed for this category's posts.
        /// </summary>
        /// <value>The feed absolute link.</value>
        public Uri FeedAbsoluteLink
        {
            get
            {
                return Utils.ConvertToAbsolute(this.FeedRelativeLink);
            }
        }

        /// <summary>
        ///     Gets the relative link to the feed for this category's posts.
        /// </summary>
        /// <value>The feed relative link.</value>
        public string FeedRelativeLink
        {
            get
            {
                return string.Format(
                    "{0}category/feed/{1}{2}",
                    Utils.RelativeWebRoot,
                    Utils.RemoveIllegalCharacters(this.Title),
                    BlogSettings.Instance.FileExtension);
            }
        }

        /// <summary>
        ///     Gets or sets the Parent ID of the object
        /// </summary>
        /// <value>The parent.</value>
        public Guid? Parent
        {
            get
            {
                return this.parent;
            }

            set
            {
                if (this.parent != value)
                {
                    this.MarkChanged("Parent");
                }

                this.parent = value;
            }
        }

        /// <summary>
        ///     Gets all posts in this category.
        /// </summary>
        /// <value>The posts.</value>
        public List<Post> Posts
        {
            get
            {
                return Post.GetPostsByCategory(this.Id);
            }
        }


        public void Save() {
            if (this.New) {
                if (this.DateCreated == DateTime.MinValue) {
                    this.DateCreated = DateTime.Now;
                }

                this.DateModified = DateTime.Now;
                BlogProvider.Provider.InsertCategory(this);
                All.Add(this);
                All.Sort();
            } else // This will be an Update
                {
                this.DateModified = DateTime.Now;
                BlogProvider.Provider.UpdateCategory(this);
                All.Sort();
            }
            this.MarkOld();
        }

        public void Delete() {
            BlogProvider.Provider.DeleteCategory(this);
            if (!All.Contains(this)) {
                return;
            }
            All.Remove(this);

            // Remode this Category from all the Posts which has it
            foreach (Post post in Posts) {
                for (int i = 0; i < post.Categories.Count; i++) {
                    if (post.Categories[i].Id == this.Id) {
                        // We didnt find the existing category in the posted category list, so it means the user has deleted the category
                        post.Categories.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the relative link to the page displaying all posts for this category.
        /// </summary>
        /// <value>The relative link.</value>
        public string RelativeLink
        {
            get
            {
                return Utils.RelativeWebRoot + "category/" + Utils.RemoveIllegalCharacters(this.Title) +
                       BlogSettings.Instance.FileExtension;
            }
        }

        /// <summary>
        ///     Gets or sets the Title or the object.
        /// </summary>
        /// <value>The title.</value>
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
        ///     Gets or sets the folder.
        /// </summary>
        /// <value>The folder.</value>
        internal static string Folder { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a category based on the specified id.
        /// </summary>
        /// <param name="id">
        /// The category id.
        /// </param>
        /// <returns>
        /// The category.
        /// </returns>
        public static Category GetCategory(Guid id)
        {
            return BlogProvider.Provider.Categories.FirstOrDefault(category => category.Id == id);
        }

        /// <summary>
        /// Returns a category based on the title
        /// </summary>
        /// <param name="id">
        /// The category name.
        /// </param>
        /// <returns>
        /// The category.
        /// </returns>
        public static Category GetCategory(string title) {
            return BlogProvider.Provider.Categories.FirstOrDefault(category => category.Title == title);
        }

        /// <summary>
        /// Gets the full title with Parent names included
        /// </summary>
        /// <returns>
        /// The complete title.
        /// </returns>
        public string CompleteTitle()
        {
            return this.parent == null
                       ? this.title
                       : string.Format("{0} - {1}", GetCategory((Guid)this.parent).CompleteTitle(), this.title);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return this.CompleteTitle();
        }

        public static List<Category> All
        {
            get {
                //    if (categories == null)
                //    {
                //        lock (SyncRoot)
                //        {
                //            if (categories == null)
                //            {
                //                categories = BlogProvider.Provider.FillCategories();
                //                categories.TrimExcess();
                //            }
                //        }
                //    }

                //    return categories;
                return BlogProvider.Provider.Categories;
            }
        }
        #endregion

        #region Implemented Interfaces

        #region IComparable<Category>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. 
        ///     The return value has the following meanings: Value Meaning Less than zero This object is 
        ///     less than the other parameter.Zero This object is equal to other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(Category other)
        {
            return this.CompleteTitle().CompareTo(other.CompleteTitle());
        }

        #endregion

        #endregion
    }
}