using System;
using System.Collections.Generic;
using blogEngine.Core.Providers;

namespace blogEngine.Core.Models
{
    /// <summary>
    /// BlogRolls are links to outside blogs.
    /// </summary>
    [Serializable]
    public class BlogRollItem :  BaseEntity<Guid>
    {
        #region Constants and Fields
        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();
        /// <summary>
        /// The blog url.
        /// </summary>
        private Uri blogUrl;

        /// <summary>
        /// The description.
        /// </summary>
        private string description;

        /// <summary>
        /// The feed url.
        /// </summary>
        private Uri feedUrl;

        /// <summary>
        /// The sort index.
        /// </summary>
        private int sortIndex;

        /// <summary>
        /// The title.
        /// </summary>
        private string title;

        /// <summary>
        /// The xfn string.
        /// </summary>
        private string xfn;


        private static List<BlogRollItem> blogRollItems;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref = "BlogRollItem" /> class.
        /// </summary>
        public BlogRollItem()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogRollItem"/> class.
        /// </summary>
        /// <param name="title">
        /// The title of the BlogRollItem.
        /// </param>
        /// <param name="description">
        /// The description of the BlogRollItem.
        /// </param>
        /// <param name="blogUrl">
        /// The <see cref="Uri"/> of the BlogRollItem.
        /// </param>
        public BlogRollItem(string title, string description, Uri blogUrl)
        {
            this.Id = Guid.NewGuid();
            this.Title = title;
            this.Description = description;
            this.BlogUrl = blogUrl;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the BlogUrl of the object.
        /// </summary>
        public Uri BlogUrl
        {
            get
            {
                return this.blogUrl;
            }

            set
            {
                if (this.blogUrl == null || !this.blogUrl.Equals(value))
                {
                    this.MarkChanged("BlogUrl");
                }

                this.blogUrl = value;
            }
        }

        /// <summary>
        ///     Gets or sets the Description of the object.
        /// </summary>
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
        ///     Gets or sets the FeedUrl of the object.
        /// </summary>
        public Uri FeedUrl
        {
            get
            {
                return this.feedUrl;
            }

            set
            {
                if (this.feedUrl == null || !this.feedUrl.Equals(value))
                {
                    this.MarkChanged("FeedUrl");
                }

                this.feedUrl = value;
            }
        }

        /// <summary>
        ///     Gets or sets the SortIndex of the object.
        /// </summary>
        public int SortIndex
        {
            get
            {
                return this.sortIndex;
            }

            set
            {
                if (this.sortIndex != value)
                {
                    this.MarkChanged("SortIndex");
                }

                this.sortIndex = value;
            }
        }

        /// <summary>
        ///     Gets or sets the Title of the object.
        /// </summary>
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
        ///     Gets or sets the Xfn of the object.
        /// </summary>
        public string Xfn
        {
            get
            {
                return this.xfn;
            }

            set
            {
                if (this.xfn != value)
                {
                    this.MarkChanged("Xfn");
                }

                this.xfn = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the BlogRollItem from the data store.
        /// </summary>
        /// <param name="id">The blogroll item id.</param>
        /// <returns>The blogroll item.</returns>
        public static BlogRollItem GetBlogRollItem(Guid id)
        {
            return BlogProvider.Provider.BlogRolls.Find(br => br.Id == id);
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

        #region Implemented Interfaces

        #region IComparable<BlogRollItem>

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
        public int CompareTo(BlogRollItem other)
        {
            return this.SortIndex.CompareTo(other.SortIndex);
        }

        public static List<BlogRollItem> All
        {
            get
            {
                if (blogRollItems == null)
                {
                    lock (SyncRoot)
                    {
                        if (blogRollItems == null)
                        {
                            blogRollItems = BlogProvider.Provider.FillBlogRoll();
                            blogRollItems.TrimExcess();
                        }
                    }
                }

                return blogRollItems;
            }
        }
        #endregion

        #endregion
    }
}