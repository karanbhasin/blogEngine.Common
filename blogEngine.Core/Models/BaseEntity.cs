using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace blogEngine.Core.Models
{
    public class BaseEntity<TKey>
    {
         #region Constants and Fields
        /// <summary>
        /// The date created.
        /// </summary>
        private DateTime dateCreated = DateTime.MinValue;

        /// <summary>
        /// The date modified.
        /// </summary>
        private DateTime dateModified = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessBase{T,TKey}"/> class. 
        /// </summary>
        protected BaseEntity()
        {
            this.New = true;
            this.IsChanged = true;
        }

        #endregion

        #region Properties
        /// <summary>
        ///     Gets or sets the date on which the instance was created.
        /// </summary>
        public DateTime DateCreated
        {
            get
            {
                return this.dateCreated == DateTime.MinValue ? this.dateCreated : this.dateCreated.AddHours(BlogSettings.Instance.Timezone);
            }

            set
            {
                if (this.dateCreated != value)
                {
                    //this.MarkChanged("DateCreated");
                }

                this.dateCreated = value;
            }
        }

        /// <summary>
        ///     Gets or sets the date on which the instance was modified.
        /// </summary>
        public DateTime DateModified
        {
            get
            {
                return this.dateModified == DateTime.MinValue ? this.dateModified : this.dateModified.AddHours(BlogSettings.Instance.Timezone);
            }

            set
            {
                this.dateModified = value;
            }
        }

        /// <summary>
        ///     Gets or sets the unique Identification of the object.
        /// </summary>
        public TKey Id { get; set; }

        /// <summary>
        ///     Gets a value indicating whether if this object's data has been changed.
        /// </summary>
        public virtual bool IsChanged { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether if this object is marked for deletion.
        /// </summary>
        public bool Deleted { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether if this is a new object, False if it is a pre-existing object.
        /// </summary>
        public bool New { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the current user is authenticated.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
        /// </value>
        protected bool Authenticated
        {
            get
            {
                return Thread.CurrentPrincipal.Identity.IsAuthenticated;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Marks an object as being dirty, or changed.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to mark dirty.
        /// </param>
        protected virtual void MarkChanged(string propertyName)
        {
            this.IsChanged = true;
            //if (!this.changedProperties.Contains(propertyName))
            //{
            //    this.changedProperties.Add(propertyName);
            //}

            //this.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Marks the object as being an clean, 
        ///     which means not dirty.
        /// </summary>
        public virtual void MarkOld()
        {
            this.IsChanged = false;
            this.New = false;
            //this.changedProperties.Clear();
        }
        #endregion
    }
}
