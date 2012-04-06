using System;
using System.Collections.Generic;

using blogEngine.Core.Providers;

namespace blogEngine.Core.Models
{
    /// <summary>
    /// Referrers are web sites that users followed to get to your blog.
    /// </summary>
    [Serializable]
    public class Referrer : BaseEntity< Guid>, IComparable<Referrer>
    {
        #region Constants and Fields

        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The referrers by day.
        /// </summary>
        private static Dictionary<DateTime, List<Referrer>> referrersByDay;

        /// <summary>
        /// The count.
        /// </summary>
        private int count;

        /// <summary>
        /// The day of the DateTime.
        /// </summary>
        private DateTime day;

        /// <summary>
        /// The possible spam.
        /// </summary>
        private bool possibleSpam;

        /// <summary>
        /// The referrer.
        /// </summary>
        private Uri referrer;

        /// <summary>
        /// The url Uri.
        /// </summary>
        private Uri url;

        private static List<Referrer> referrers;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref = "Referrer" /> class.
        /// </summary>
        public Referrer()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Referrer"/> class.
        /// </summary>
        /// <param name="referrer">
        /// The ReferrerUrl for the Referrer.
        /// </param>
        public Referrer(Uri referrer)
            : this()
        {
            this.ReferrerUrl = referrer;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets all of the Referrers from the data store.
        /// </summary>
        public static List<Referrer> All
        {
            get
            {
                if (referrers == null)
                {
                    lock (SyncRoot)
                    {
                        if (referrers == null)
                        {
                            referrers = BlogProvider.Provider.FillReferrers();
                            referrers.TrimExcess();
                        }
                    }
                }

                return referrers;
            }
        }

        /// <summary>
        ///     Gets an automatically maintained Dictionary of Referrers separated by Day.
        /// </summary>
        public static Dictionary<DateTime, List<Referrer>> ReferrersByDay
        {
            get
            {
                if (referrersByDay == null)
                {
                    referrersByDay = new Dictionary<DateTime, List<Referrer>>();
                    foreach (var refer in BlogProvider.Provider.Referrers)
                    {
                        if (referrersByDay.ContainsKey(refer.Day))
                        {
                            referrersByDay[refer.Day].Add(refer);
                        }
                        else
                        {
                            referrersByDay.Add(refer.Day, new List<Referrer> { refer });
                        }
                    }
                }
                return referrersByDay;
            }
        }

        /// <summary>
        ///     Gets or sets the Count of the object.
        /// </summary>
        public int Count
        {
            get
            {
                return this.count;
            }

            set
            {
                if (this.count != value)
                {
                    this.MarkChanged("Count");
                }

                this.count = value;
            }
        }

        /// <summary>
        ///     Gets or sets the Day of the object.
        /// </summary>
        public DateTime Day
        {
            get
            {
                return this.day;
            }

            set
            {
                if (!this.day.Equals(value))
                {
                    this.MarkChanged("Day");
                }

                this.day = value;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the referrer is possibly spam.
        /// </summary>
        public bool PossibleSpam
        {
            get
            {
                return this.possibleSpam;
            }

            set
            {
                if (this.possibleSpam != value)
                {
                    this.MarkChanged("PossibleSpam");
                }

                this.possibleSpam = value;
            }
        }

        /// <summary>
        ///     Gets or sets the referrer address of the object.
        /// </summary>
        public Uri ReferrerUrl
        {
            get
            {
                return this.referrer;
            }

            set
            {
                if (this.referrer == null || !this.referrer.Equals(value))
                {
                    this.MarkChanged("Referrer");
                }

                this.referrer = value;
            }
        }

        /// <summary>
        ///     Gets or sets the referrer Url of the object.
        /// </summary>
        public Uri Url
        {
            get
            {
                return this.url;
            }

            set
            {
                if (this.url == null || !this.url.Equals(value))
                {
                    this.MarkChanged("Url");
                }

                this.url = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return this.ReferrerUrl.ToString();
        }

        #endregion

        #region Implemented Interfaces

        #region IComparable<Referrer>

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
        public int CompareTo(Referrer other)
        {
            var compareThis = string.Format("{0} {1}", this.ReferrerUrl, this.Url);
            var compareOther = string.Format("{0} {1}", other.ReferrerUrl, other.Url);
            return compareThis.CompareTo(compareOther);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// The add referrer.
        /// </summary>
        /// <param name="referrer">
        /// The referrer.
        /// </param>
        private static void AddReferrer(Referrer referrer)
        {
            List<Referrer> day;
            if (ReferrersByDay.ContainsKey(referrer.Day))
            {
                day = ReferrersByDay[referrer.Day];
            }
            else
            {
                day = new List<Referrer>();
                ReferrersByDay.Add(referrer.Day, day);
            }

            if (!day.Contains(referrer))
            {
                day.Add(referrer);
            }
        }

        #endregion
    }
}