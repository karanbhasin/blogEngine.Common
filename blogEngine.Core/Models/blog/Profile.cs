using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Security;
using System.Xml.Serialization;
using blogEngine.Core.Providers;

namespace blogEngine.Core.Models
{
    /// <summary>
    /// This business object is to handle the profiles of users
    /// </summary>
    [XmlRoot("profile")]
    public class Profile : BaseEntity<string>
    {
        #region Fields
        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        private string aboutMe;
        private string birthDate;
        private string cityState;
        private string country;
        private string displayName;
        private string firstName;
        private string gender;
        private string interests;
        private bool isPrivate;
        private string lastName;
        private string photoURL;
        private string regionState;
        private string userName;
        private static List<Profile> profiles;
        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("displayName", DataType = "string")]
        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("interests", DataType = "string")]
        public string Interests
        {
            get { return interests; }
            set { interests = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("userName", DataType = "string")]
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        ///<summary>
        ///</summary>
        [XmlElement("aboutme", DataType = "string")]
        public string AboutMe
        {
            get { return aboutMe; }
            set { aboutMe = value; }
        }

        ///<summary>
        ///</summary>
        [XmlElement("firstName", DataType = "string")]
        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        ///<summary>
        ///</summary>
        [XmlElement("lastName", DataType = "string")]
        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        ///<summary>
        ///</summary>
        [XmlElement("isPrivate", DataType = "bool")]
        public bool IsPrivate
        {
            get { return isPrivate; }
            set { isPrivate = value; }
        }

        ///<summary>
        ///</summary>
        [XmlElement("photoURL", DataType = "string")]
        public string PhotoURL
        {
            get { return photoURL; }
            set { photoURL = value; }
        }

        ///<summary>
        ///</summary>
        [XmlElement("gender", DataType = "string")]
        public string Gender
        {
            get { return gender; }
            set { gender = value; }
        }

        ///<summary>
        ///</summary>
        [XmlElement("birthDate", DataType = "string")]
        public string BirthDate
        {
            get { return birthDate; }
            set { birthDate = value; }
        }

        ///<summary>
        ///</summary>
        [XmlElement("cityState", DataType = "string")]
        public string CityState
        {
            get { return cityState; }
            set { cityState = value; }
        }

        ///<summary>
        ///</summary>
        [XmlElement("regionState", DataType = "string")]
        public string RegionState
        {
            get { return regionState; }
            set { regionState = value; }
        }

        ///<summary>
        ///</summary>
        [XmlElement("country", DataType = "string")]
        public string Country
        {
            get { return country; }
            set { country = value; }
        }

        #endregion

        #region Helpers

        ///<summary>
        ///</summary>
        ///<param name="username"></param>
        ///<returns></returns>
        public static Profile GetProfile(string username)
        {
            return BlogProvider.Provider.SelectProfile(username);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userProfile"></param>
        public static void SaveProfile(Profile userProfile)
        {
            BlogProvider.Provider.InsertProfile(userProfile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<Profile> GetProfiles()
        {
            return All;
        }

        public static List<Profile> All
        {
            get
            {
                if (profiles == null)
                {
                    lock (SyncRoot)
                    {
                        if (profiles == null)
                        {
                            profiles = BlogProvider.Provider.FillProfiles();
                            profiles.TrimExcess();
                        }
                    }
                }

                return profiles;
            }
        }
        #endregion

    }
}