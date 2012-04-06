namespace blogEngine.Core.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The role class.
    /// </summary>
    public class Role
    {
        #region Constants and Fields

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Role"/> class.
        /// </summary>
        /// <param name="name">
        /// A name of the role.
        /// </param>
        public Role(string name)
        {
            this.Name = name;
            this.Users = new List<string>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "Role" /> class.
        /// </summary>
        public Role()
        {
            this.Users = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Role"/> class.
        /// </summary>
        /// <param name="name">
        /// A name of the role.
        /// </param>
        /// <param name="userNames">
        /// A list of users in role.
        /// </param>
        public Role(string name, List<string> userNames)
        {
            this.Name = name;
            this.Users = userNames;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>The name of the role.</value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets the users.
        /// </summary>
        /// <value>The users.</value>
        public List<string> Users { get; private set; }

        #endregion
    }
}