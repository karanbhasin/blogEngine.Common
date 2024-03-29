﻿using System;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Security.Cryptography;
using blogEngine.Core.Providers;

namespace blogEngine.Core.Models
{
    /// <summary>
    /// Represents a comment to a blog post.
    /// </summary>
    [Serializable]
    public sealed class Comment : BaseEntity<Guid>, IComparable<Comment>
    {
        #region Constants and Fields

        /// <summary>
        /// The comments.
        /// </summary>
        private List<Comment> comments;

        /// <summary>
        /// The date created.
        /// </summary>
        private DateTime dateCreated = DateTime.MinValue;

        /// <summary>
        /// String representing avatar image.
        /// </summary>
        private string avatar;

        #endregion


        #region Properties

        /// <summary>
        ///     Gets the absolute link.
        /// </summary>
        /// <value>The absolute link.</value>
        public Uri AbsoluteLink
        {
            get
            {
                return new Uri(string.Format("{0}#id_{1}", this.Parent.AbsoluteLink, this.Id));
            }
        }

        /// <summary>
        ///     Gets or sets the author.
        /// </summary>
        /// <value>The author.</value>
        public string Author { get; set; }

        /// <summary>
        ///     Gets or sets the Avatar of the comment.
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        ///     Gets a list of Comments.
        /// </summary>
        public List<Comment> Comments
        {
            get
            {
                return this.comments ?? (this.comments = new List<Comment>());
            }
        }

        /// <summary>
        ///     Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        public string Content { get; set; }

        /// <summary>
        ///     Gets or sets the country.
        /// </summary>
        /// <value>The country.</value>
        public string Country { get; set; }

        /// <summary>
        ///     Gets or sets when the comment was created.
        /// </summary>
        //public DateTime DateCreated
        //{
        //    get
        //    {
        //        return this.dateCreated == DateTime.MinValue ? this.dateCreated : this.dateCreated.AddHours(BlogSettings.Instance.Timezone);
        //    }

        //    set
        //    {
        //        this.dateCreated = value;
        //    }
        //}

        /// <summary>
        ///     Gets the description. Returns always string.empty.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }

        /// <summary>
        ///     Gets or sets the IP address.
        /// </summary>
        /// <value>The IP address.</value>
        public string IP { get; set; }

        /// <summary>
        ///     Gets or sets the Id of the comment.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the Comment is approved.
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        ///     Indicate if comment is spam
        /// </summary>
        public bool IsSpam { get; set; }

        /// <summary>
        ///     indicate if comment is deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        ///     Gets a value indicating whether or not this comment has been published
        /// </summary>
        public bool Published
        {
            get
            {
                return IsApproved && !IsSpam && !IsDeleted;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether or not this comment should be shown
        /// </summary>
        /// <value></value>
        public bool Visible
        {
            get
            {
                return IsApproved && !IsSpam && !IsDeleted;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether or not this comment is visible to visitors not logged into the blog.
        /// </summary>
        /// <value></value>
        public bool VisibleToPublic
        {
            get
            {
                return IsApproved && !IsSpam && !IsDeleted;
            }
        }

        /// <summary>
        ///    Gets or sets process that approved or rejected comment
        /// </summary>
        [XmlElement]
        public string ModeratedBy { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public Post Parent { get; set; }

        /// <summary>
        ///     Gets or sets the Id of the parent comment.
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        ///     Gets the relative link of the comment.
        /// </summary>
        /// <value>The relative link.</value>
        public string RelativeLink
        {
            get
            {
                return string.Format("{0}#id_{1}", this.Parent.RelativeLink, this.Id);
            }
        }

        /// <summary>
        ///     Gets abbreviated content
        /// </summary>
        public string Teaser
        {
            get
            {
                var ret = Utils.StripHtml(this.Content).Trim();
                return ret.Length > 120 ? string.Format("{0} ...", ret.Substring(0, 116)) : ret;
            }
        }

        /// <summary>
        /// Pingbacks and trackbacks are identified by email
        /// </summary>
        public bool IsPingbackOrTrackback
        {
            get { return (this.Email.ToLowerInvariant() == "pingback" || this.Email.ToLowerInvariant() == "trackback") ? true : false; }
        }

        /// <summary>
        ///     Gets the title of the object
        /// </summary>
        /// <value></value>
        public string Title
        {
            get
            {
                //if (this.Website != null)
                //{
                //    return string.Format("<a class=\"comment_auth\" href=\"{2}\" alt=\"{2}\" title=\"{2}\">{0}</a> on <a href=\"{3}\" alt=\"{3}\">{1}</a>", this.Author, this.Parent.Title, this.Website.ToString(), this.RelativeLink);
                //}
                //return string.Format("{0} on <a href=\"{2}\" alt=\"{2}\">{1}</a>", this.Author, this.Parent.Title, this.RelativeLink);
                if (string.IsNullOrEmpty(this.Author) && string.IsNullOrEmpty(this.Email))
                    return "Anonymous";
                return string.IsNullOrEmpty(this.Author) ? this.Email : this.Author;
            }
        }

        /// <summary>
        ///     Gets or sets the website.
        /// </summary>
        /// <value>The website.</value>
        public Uri Website { get; set; }

        /// <summary>
        /// Gets Categories.
        /// </summary>
        StateList<Category> Categories
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets DateModified.
        /// </summary>
        //DateTime DateModified
        //{
        //    get
        //    {
        //        return this.DateCreated;
        //    }
        //}

        #endregion

        #region Implemented Interfaces

        #region IComparable<Comment>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the 
        ///     objects being compared. The return value has the following meanings: 
        ///     Value Meaning Less than zero This object is less than the other parameter.
        ///     Zero This object is equal to other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(Comment other)
        {
            return this.DateCreated.CompareTo(other.DateCreated);
        }

        #endregion

        #endregion
    }
}