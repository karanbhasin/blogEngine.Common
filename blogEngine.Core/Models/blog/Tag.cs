using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using blogEngine.Core.Providers;

namespace blogEngine.Core.Models
{
    [Serializable]
    public class Tag : BaseEntity<Guid>, IComparable<Tag> // For tag.Sort
    {
        #region Constants and Fields
        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        ///     The title.
        /// </summary>
        private string text;

        private static List<Tag> tags;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="Tag"/> class. 
        /// </summary>
        static Tag()
        {
            Folder = HttpContext.Current.Server.MapPath(BlogSettings.Instance.StorageLocation);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "Tag" /> class.
        /// </summary>
        public Tag()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        ///     The tag.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        public Tag(string text)
        {
            this.text = text;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the absolute link to the page displaying all posts for this tag.
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
        ///     Gets or sets the Text of the object.
        /// </summary>
        /// <value>The Text.</value>
        public string Text
        {
            get
            {
                return this.text;
            }

            set
            {
               this.text = value;
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
                return Post.GetPostsByKeyword(this.Id);
            }
        }


        public void Save() {
            if (this.New) {
                if (this.DateCreated == DateTime.MinValue) {
                    this.DateCreated = DateTime.Now;
                }

                this.DateModified = DateTime.Now;
                BlogProvider.Provider.InsertTag(this);
                
                All.Add(this);
                All.Sort();

            } else // This will be an Update
                {
                this.DateModified = DateTime.Now;
            }
            this.MarkOld();
        }

        public void Delete() {
            BlogProvider.Provider.DeleteTag(this);
            if (!All.Contains(this)) {
                return;
            }
            All.Remove(this);

            // Remode this Category from all the Posts which has it
            foreach (Post post in Posts) {
                for (int i = 0; i < post.Keywords.Count; i++) {
                    if (post.Keywords[i].Id == this.Id) {
                        // We didnt find the existing category in the posted category list, so it means the user has deleted the category
                        post.Keywords.RemoveAt(i);
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
                return Utils.RelativeWebRoot + "tag/" + Utils.RemoveIllegalCharacters(this.text) +
                       BlogSettings.Instance.FileExtension;
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
        /// Returns a tag based on the specified id.
        /// </summary>
        /// <param name="id">
        /// The tag id.
        /// </param>
        /// <returns>
        /// The tag.
        /// </returns>
        public static Tag GetTag(Guid id)
        {
            return BlogProvider.Provider.Tags.FirstOrDefault(tag => tag.Id == id);
        }


        /// <summary>
        /// Returns a tag with the specified name
        /// </summary>
        /// <param name="tag">
        /// The tag to find.
        /// </param>
        /// <returns>
        /// The tag.
        /// </returns>
        public static Tag GetTag(string tag) {
            return BlogProvider.Provider.Tags.FirstOrDefault(t => t.Text == tag);
        }

        
        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return this.Text;
        }

        public static List<Tag> All
        {
            get
            {
                //if (tags == null)
                //{
                //    lock (SyncRoot)
                //    {
                //        if (tags == null)
                //        {
                //            tags = BlogProvider.Provider.FillTags();
                //            tags.TrimExcess();
                //        }
                //    }
                //}

                //return tags;
                return BlogProvider.Provider.Tags;
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
        public int CompareTo(Tag other)
        {
            return this.Text.CompareTo(other.Text);
        }

        #endregion

        #endregion
    }
}