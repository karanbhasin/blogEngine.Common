﻿namespace BlogEngine.Core.Providers
{
    // Written by: Roman D. Clarkson
    // http://www.romanclarkson.com  inspirit@romanclarkson.com
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.IO;
    using System.Linq;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Security;
    using System.Xml;

    /// <summary>
    /// The xml role provider.
    /// </summary>
    public class XmlRoleProvider : RoleProvider
    {
        #region Constants and Fields

        /// <summary>
        /// The default roles to add.
        /// </summary>
        private readonly string[] defaultRolesToAdd = new[] { BlogSettings.Instance.AdministratorRole };

        /// <summary>
        /// The roles.
        /// </summary>
        private readonly List<Role> roles = new List<Role>();

        /// <summary>
        /// The user names.
        /// </summary>
        private List<string> userNames;

        /// <summary>
        /// The xml file name.
        /// </summary>
        private string xmlFileName;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <returns>
        ///     The name of the application to store and retrieve role information for.
        /// </returns>
        public override string ApplicationName
        {
            get
            {
                return "BlogEngine.NET";
            }

            set
            {
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the specified user names to the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">
        /// A string array of user names to be added to the specified roles. 
        /// </param>
        /// <param name="roleNames">
        /// A string array of the role names to add the specified user names to. 
        /// </param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            var currentRoles = new List<string>(this.GetAllRoles());
            if (usernames.Length != 0 && roleNames.Length != 0)
            {
                foreach (var rolename in roleNames.Where(rolename => !currentRoles.Contains(rolename)))
                {
                    this.roles.Add(new Role(rolename, new List<string>(usernames)));
                }

                foreach (var role in this.roles)
                {
                    var role1 = role;
                    foreach (var s in from name in roleNames
                                      where role1.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                                      from s in usernames
                                      where !role1.Users.Contains(s)
                                      select s)
                    {
                        role.Users.Add(s);
                    }
                }
            }

            this.Save();
        }

        /// <summary>
        /// Adds a new role to the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">
        /// The name of the role to create.
        /// </param>
        public override void CreateRole(string roleName)
        {
            if (this.roles.Contains(new Role(roleName)))
            {
                return;
            }

            this.roles.Add(new Role(roleName));
            this.Save();
        }

        /// <summary>
        /// Removes a role from the data source for the configured applicationName.
        /// </summary>
        /// <returns>
        /// true if the role was successfully deleted; otherwise, false.
        /// </returns>
        /// <param name="roleName">
        /// The name of the role to delete.
        /// </param>
        /// <param name="throwOnPopulatedRole">
        /// If true, throw an exception if roleName has one or more members and do not delete roleName.
        /// </param>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!roleName.Equals(BlogSettings.Instance.AdministratorRole, StringComparison.OrdinalIgnoreCase))
            {
                for (var i = 0; i < this.roles.Count; i++)
                {
                    if (this.roles[i].Name != roleName)
                    {
                        continue;
                    }

                    this.roles.RemoveAt(i);
                    this.Save();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the users where the user name matches usernameToMatch and the user is a member of the specified role.
        /// </returns>
        /// <param name="roleName">
        /// The role to search in.
        /// </param>
        /// <param name="usernameToMatch">
        /// The user name to search for.
        /// </param>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            var usersInRole = new List<string>();
            if (this.IsUserInRole(usernameToMatch, roleName))
            {
                usersInRole.AddRange(this.userNames);
            }

            return usersInRole.ToArray();
        }

        /// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source for the configured applicationName.
        /// </returns>
        public override string[] GetAllRoles()
        {
            return this.roles.Select(role => role.Name).ToArray();
        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        /// <param name="username">
        /// The user to return a list of roles for.
        /// </param>
        public override string[] GetRolesForUser(string username)
        {
            // ReadRoleDataStore();
            return (from role in this.roles
                    from user in role.Users
                    where user.Equals(username, StringComparison.OrdinalIgnoreCase)
                    select role.Name).ToArray();
        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        /// </returns>
        /// <param name="roleName">
        /// The name of the role to get the list of users for. 
        /// </param>
        public override string[] GetUsersInRole(string roleName)
        {
            // ReadRoleDataStore();
            return (from role in this.roles
                    where role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)
                    from user in role.Users
                    select user.ToLowerInvariant()).ToArray();
        }

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">
        /// The friendly name of the provider.
        /// </param>
        /// <param name="config">
        /// A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The name of the provider is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The name of the provider has a length of zero.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
        /// </exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            this.ReadMembershipDataStore();
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (String.IsNullOrEmpty(name))
            {
                name = "XmlMembershipProvider";
            }

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "XML role provider");
            }

            base.Initialize(name, config);

            // Initialize _XmlFileName and make sure the path
            // is app-relative
            var path = config["xmlFileName"];

            if (String.IsNullOrEmpty(path))
            {
                path = string.Format("{0}roles.xml", BlogSettings.Instance.StorageLocation);
            }

            if (!VirtualPathUtility.IsAppRelative(path))
            {
                throw new ArgumentException("xmlFileName must be app-relative");
            }

            var fullyQualifiedPath =
                VirtualPathUtility.Combine(
                    VirtualPathUtility.AppendTrailingSlash(HttpRuntime.AppDomainAppVirtualPath), path);

            this.xmlFileName = HostingEnvironment.MapPath(fullyQualifiedPath);
            config.Remove("xmlFileName");

            // Make sure we have permission to read the XML data source and
            // throw an exception if we don't
            var permission = new FileIOPermission(FileIOPermissionAccess.Write, this.xmlFileName);
            permission.Demand();

            if (!File.Exists(this.xmlFileName))
            {
                this.AddUsersToRoles(this.userNames.ToArray(), this.defaultRolesToAdd);
            }

            // Now that we know a xml file exists we can call it.
            this.ReadRoleDataStore();

            if (!this.RoleExists(BlogSettings.Instance.AdministratorRole))
            {
                this.AddUsersToRoles(this.userNames.ToArray(), this.defaultRolesToAdd);
            }

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
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        /// <param name="username">
        /// The user name to search for.
        /// </param>
        /// <param name="roleName">
        /// The role to search in.
        /// </param>
        public override bool IsUserInRole(string username, string roleName)
        {
            return
                this.roles.Where(role => role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).SelectMany(
                    role => role.Users).Any(user => user.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">
        /// A string array of user names to be removed from the specified roles. 
        /// </param>
        /// <param name="roleNames">
        /// A string array of role names to remove the specified user names from. 
        /// </param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (usernames.Length != 0 && roleNames.Length != 0)
            {
                foreach (var role in this.roles)
                {
                    var role1 = role;
                    if (!roleNames.Any(name => role1.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    foreach (var user in usernames)
                    {
                        if (role.Name.Equals(
                            BlogSettings.Instance.AdministratorRole, StringComparison.OrdinalIgnoreCase))
                        {
                            if (role.Users.Count != 1)
                            {
                                // if (role.Users.Contains(user))
                                // role.Users.Remove(user);
                                RemoveItemFromList(role.Users, user);
                            }
                        }
                        else
                        {
                            // if (role.Users.Contains(user))
                            // role.Users.Remove(user);
                            RemoveItemFromList(role.Users, user);
                        }
                    }
                }
            }

            this.Save();
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        /// </returns>
        /// <param name="roleName">
        /// The name of the role to search for in the data source. 
        /// </param>
        public override bool RoleExists(string roleName)
        {
            var currentRoles = new List<string>(this.GetAllRoles());
            return currentRoles.Contains(roleName) ? true : false;
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            var settings = new XmlWriterSettings { Indent = true };

            using (var writer = XmlWriter.Create(this.xmlFileName, settings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("roles");

                foreach (var role in this.roles)
                {
                    writer.WriteStartElement("role");
                    writer.WriteElementString("name", role.Name);
                    writer.WriteStartElement("users");
                    foreach (var username in role.Users)
                    {
                        writer.WriteElementString("user", username);
                    }

                    writer.WriteEndElement(); // closes users
                    writer.WriteEndElement(); // closes role
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The remove item from list.
        /// </summary>
        /// <param name="list">
        /// The list of string.
        /// </param>
        /// <param name="item">
        /// The item of string.
        /// </param>
        private static void RemoveItemFromList(ICollection<string> list, string item)
        {
            if (list == null || string.IsNullOrEmpty(item) || list.Count == 0)
            {
                return;
            }

            var usersToRemove = list.Where(u => u.Equals(item, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var u in usersToRemove)
            {
                list.Remove(u);
            }
        }

        /// <summary>
        /// Only so we can add users to the adminstrators role.
        /// </summary>
        private void ReadMembershipDataStore()
        {
            var fullyQualifiedPath =
                VirtualPathUtility.Combine(
                    VirtualPathUtility.AppendTrailingSlash(HttpRuntime.AppDomainAppVirtualPath), 
                    BlogSettings.Instance.StorageLocation + "users.xml");

            lock (this)
            {
                if (this.userNames != null)
                {
                    return;
                }

                this.userNames = new List<string>();
                var doc = new XmlDocument();
                if (fullyQualifiedPath != null)
                {
                    var thepath = HostingEnvironment.MapPath(fullyQualifiedPath);
                    if (thepath != null)
                    {
                        doc.Load(thepath);
                    }
                }

                var nodes = doc.GetElementsByTagName("User");

                foreach (var userName in
                    nodes.Cast<XmlNode>().Select(node => node["UserName"]).Where(userName => userName != null))
                {
                    this.userNames.Add(userName.InnerText);
                }
            }
        }

        /// <summary>
        /// Builds the internal cache of users.
        /// </summary>
        private void ReadRoleDataStore()
        {
            lock (this)
            {
                var doc = new XmlDocument();

                try
                {
                    doc.Load(this.xmlFileName);
                    var nodes = doc.GetElementsByTagName("role");
                    foreach (XmlNode roleNode in nodes)
                    {
                        var name = roleNode.SelectSingleNode("name");
                        if (name == null)
                        {
                            continue;
                        }

                        var tempRole = new Role(name.InnerText);
                        var user = roleNode.SelectNodes("users/user");
                        if (user != null)
                        {
                            foreach (XmlNode userNode in user)
                            {
                                tempRole.Users.Add(userNode.InnerText);
                            }
                        }

                        this.roles.Add(tempRole);
                    }
                }
                catch (XmlException)
                {
                    this.AddUsersToRoles(this.userNames.ToArray(), this.defaultRolesToAdd);
                }
            }
        }

        #endregion
    }
}