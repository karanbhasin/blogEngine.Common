using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Configuration;
using System.Web.Configuration;
using blogEngine.Core.Models;

namespace blogEngine.Core.Providers
{
    /// <summary>
    /// A base class for all custom providers to inherit from.
    /// </summary>
    public abstract class BlogProvider : ProviderBase
    {
        #region Constants and Fields

        /// <summary>
        /// The lock object.
        /// </summary>
        private static readonly object TheLock = new object();

        /// <summary>
        /// The providers.
        /// </summary>
        private static BlogProviderCollection providers;

        /// <summary>
        /// The provider.
        /// </summary>
        private static BlogProvider provider;

        #endregion

        #region Provider
        /// <summary>
        /// Load the providers from the web.config.
        /// </summary>
        private static void LoadProviders()
        {
            // Avoid claiming lock if providers are already loaded
            if (provider == null)
            {
                lock (TheLock)
                {
                    // Do this again to make sure _provider is still null
                    if (provider == null)
                    {
                        //// Get a reference to the <blogProvider> section
                        //var section = (BlogProviderSection)WebConfigurationManager.GetSection("BlogEngine/blogProvider");

                        //// Load registered providers and point _provider
                        //// to the default provider
                        //providers = new BlogProviderCollection();
                        //ProvidersHelper.InstantiateProviders(section.Providers, providers, typeof(BlogProvider));
                        //provider = providers[section.DefaultProvider];

                        //if (provider == null)
                        //{
                        //    throw new ProviderException("Unable to load default BlogProvider");
                        //}

                        //                      <configSections>
                        //  <sectionGroup name="BlogEngine">
                        //    <section name="blogProvider" requirePermission="false" type="blogEngine.Core.Providers.BlogProviderSection, BlogEngine.Core" allowDefinition="MachineToApplication" restartOnExternalChanges="true"/>
                        //  </sectionGroup>
                        //</configSections>

                        //  <BlogEngine>
                        //    <blogProvider defaultProvider="XmlBlogProvider">
                        //      <providers>
                        //        <add name="XmlBlogProvider" type="BlogEngine.Core.Providers.XmlBlogProvider, BlogEngine.Core"/>
                        //        <add name="DbBlogProvider" type="BlogEngine.Core.Providers.DbBlogProvider, BlogEngine.Core" connectionStringName="BlogEngine"/>
                        //      </providers>
                        //    </blogProvider>
                        //  </BlogEngine>
                        provider = new XmlBlogProvider();
                    }
                }
            }
        }


        public static BlogProvider Provider
        {
            get
            {
                LoadProviders();
                return provider;
            }
        }
        #endregion

        // Post
        #region Public Methods

        /// <summary>
        /// Deletes a BlogRoll from the data store specified by the provider.
        /// </summary>
        /// <param name="blogRollItem">
        /// The blog Roll Item to delete.
        /// </param>
        public abstract void DeleteBlogRollItem(BlogRollItem blogRollItem);

        /// <summary>
        /// Deletes a Category from the data store specified by the provider.
        /// </summary>
        /// <param name="category">
        /// The category to delete.
        /// </param>
        public abstract void DeleteCategory(Category category);

        public abstract void DeleteTag(Tag tag);
        /// <summary>
        /// Deletes a Page from the data store specified by the provider.
        /// </summary>
        /// <param name="page">
        /// The page to delete.
        /// </param>
        public abstract void DeletePage(Page page);

        /// <summary>
        /// Deletes a Post from the data store specified by the provider.
        /// </summary>
        /// <param name="post">
        /// The post to delete.
        /// </param>
        public abstract void DeletePost(Post post);

        /// <summary>
        /// Deletes a Page from the data store specified by the provider.
        /// </summary>
        /// <param name="profile">
        /// The profile to delete.
        /// </param>
        public abstract void DeleteProfile(AuthorProfile profile);

        public abstract List<BlogRollItem> BlogRolls { get; }

        public abstract List<Category> Categories { get; }

        public abstract List<Tag> Tags { get; }

        public abstract List<Page> Pages { get; }

        public abstract List<Post> Posts { get; }

        public abstract void Reload(int what);
        public abstract List<Post> AllPosts { get; }

        public abstract List<Post> PhotoPosts { get; }

        public abstract List<AuthorProfile> Profiles { get; }

        //public abstract List<Profile> Profiles { get; }

        public abstract List<Referrer> Referrers { get; }

        /// <summary>
        /// Retrieves all BlogRolls from the provider and returns them in a list.
        /// </summary>
        /// <returns>A list of BlogRollItem.</returns>
        public abstract List<BlogRollItem> FillBlogRoll();

        /// <summary>
        /// Retrieves all Categories from the provider and returns them in a List.
        /// </summary>
        /// <returns>A list of Category.</returns>
        public abstract List<Category> FillCategories();

        public abstract List<Tag> FillTags();
        /// <summary>
        /// Retrieves all Pages from the provider and returns them in a List.
        /// </summary>
        /// <returns>A list of Page.</returns>
        public abstract List<Page> FillPages();


        /// <summary>
        /// Retrieves all Posts from the provider and returns them in a List.
        /// </summary>
        /// <param name="forPhotoPost">If true, returns all PhotoPosts</param>
        /// <returns>A list of Post.</returns>
        public abstract List<Post> FillPosts();

        /// <summary>
        /// Retrieves all Pages from the provider and returns them in a List.
        /// </summary>
        /// <returns>A list of AuthorProfile.</returns>
        public abstract List<AuthorProfile> FillProfiles();

        //public abstract List<Profile> FillProfiles();

        /// <summary>
        /// Deletes a Referrer from the data store specified by the provider.
        /// </summary>
        /// <returns>A list of Referrer.</returns>
        public abstract List<Referrer> FillReferrers();

        /// <summary>
        /// Inserts a new BlogRoll into the data store specified by the provider.
        /// </summary>
        /// <param name="blogRollItem">
        /// The blog Roll Item.
        /// </param>
        public abstract void InsertBlogRollItem(BlogRollItem blogRollItem);

        /// <summary>
        /// Inserts a new Category into the data store specified by the provider.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        public abstract void InsertCategory(Category category);

        public abstract void InsertTag(Tag tag);
        /// <summary>
        /// Inserts a new Page into the data store specified by the provider.
        /// </summary>
        /// <param name="page">
        /// The page to insert.
        /// </param>
        public abstract void InsertPage(Page page);

        /// <summary>
        /// Inserts a new Post into the data store specified by the provider.
        /// </summary>
        /// <param name="post">
        /// The post to insert.
        /// </param>
        public abstract void InsertPost(Post post);

        /// <summary>
        /// Inserts a new Page into the data store specified by the provider.
        /// </summary>
        /// <param name="profile">
        /// The profile to insert.
        /// </param>
        public abstract void InsertProfile(AuthorProfile profile);

        //public abstract void InsertProfile(Profile userProfile);

        

        /// <summary>
        /// Inserts a new Referrer into the data store specified by the provider.
        /// </summary>
        /// <param name="referrer">
        /// The referrer to insert.
        /// </param>
        public abstract void InsertReferrer(Referrer referrer);



        /// <summary>
        /// Retrieves a BlogRoll from the provider based on the specified id.
        /// </summary>
        /// <param name="id">The Blog Roll Item Id.</param>
        /// <returns>A BlogRollItem.</returns>
        public abstract BlogRollItem SelectBlogRollItem(Guid id);

        /// <summary>
        /// Retrieves a Category from the provider based on the specified id.
        /// </summary>
        /// <param name="id">The Category id.</param>
        /// <returns>A Category.</returns>
        public abstract Category SelectCategory(Guid id);

        public abstract Tag SelectTag(Guid id);

        /// <summary>
        /// Retrieves a Page from the provider based on the specified id.
        /// </summary>
        /// <param name="id">The Page id.</param>
        /// <returns>The Page object.</returns>
        public abstract Page SelectPage(Guid id);

        /// <summary>
        /// Retrieves a Post from the provider based on the specified id.
        /// </summary>
        /// <param name="id">The Post id.</param>
        /// <returns>A Post object.</returns>
        public abstract Post SelectPost(Guid id);

        /// <summary>
        /// Retrieves a Page from the provider based on the specified id.
        /// </summary>
        /// <param name="id">The AuthorProfile id.</param>
        /// <returns>An AuthorProfile.</returns>
        public abstract AuthorProfile SelectProfile(string id);

         ///<summary>
        ///</summary>
        ///<param name="username"></param>
        ///<returns></returns>
        //public abstract Profile SelectProfile(string username);

       
       
        /// <summary>
        /// Retrieves a Referrer from the provider based on the specified id.
        /// </summary>
        /// <param name="id">The Referrer Id.</param>
        /// <returns>A Referrer.</returns>
        public abstract Referrer SelectReferrer(Guid id);

        /// <summary>
        /// Gets the storage location.
        /// </summary>
        /// <returns>The storage location.</returns>
        public abstract string StorageLocation();

        /// <summary>
        /// Updates an existing BlogRoll in the data store specified by the provider.
        /// </summary>
        /// <param name="blogRollItem">
        /// The blog Roll Item to update.
        /// </param>
        public abstract void UpdateBlogRollItem(BlogRollItem blogRollItem);

        /// <summary>
        /// Updates an existing Category in the data store specified by the provider.
        /// </summary>
        /// <param name="category">
        /// The category to update.
        /// </param>
        public abstract void UpdateCategory(Category category);

        public abstract void UpdateTag(Tag tag);
        /// <summary>
        /// Updates an existing Page in the data store specified by the provider.
        /// </summary>
        /// <param name="page">
        /// The page to update.
        /// </param>
        public abstract void UpdatePage(Page page);

        /// <summary>
        /// Updates an existing Post in the data store specified by the provider.
        /// </summary>
        /// <param name="post">
        /// The post to update.
        /// </param>
        public abstract void UpdatePost(Post post);

        /// <summary>
        /// Updates an existing Page in the data store specified by the provider.
        /// </summary>
        /// <param name="profile">
        /// The profile to update.
        /// </param>
        public abstract void UpdateProfile(AuthorProfile profile);

        /// <summary>
        /// Updates an existing Referrer in the data store specified by the provider.
        /// </summary>
        /// <param name="referrer">
        /// The referrer to update.
        /// </param>
        public abstract void UpdateReferrer(Referrer referrer);

        #endregion

        #region Advanced
        /// <summary>
        /// Loads the settings from the provider.
        /// </summary>
        /// <returns>A StringDictionary.</returns>
        public abstract StringDictionary LoadSettings();

        /// <summary>
        /// Saves the settings to the provider.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public abstract void SaveSettings(StringDictionary settings);

        /// <summary>
        /// Loads the ping services.
        /// </summary>
        /// <returns>
        /// A StringCollection.
        /// </returns>
        public abstract StringCollection LoadPingServices();

        /// <summary>
        /// Saves the ping services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public abstract void SavePingServices(StringCollection services);

        /// <summary>
        /// Loads the stop words used in the search feature.
        /// </summary>
        /// <returns>
        /// A StringCollection.
        /// </returns>
        public abstract StringCollection LoadStopWords();
        /*
        /// <summary>
        /// Loads settings from data store
        /// </summary>
        /// <param name="extensionType">
        /// Extension Type
        /// </param>
        /// <param name="extensionId">
        /// Extensio Id
        /// </param>
        /// <returns>
        /// Settings as stream
        /// </returns>
        public abstract object LoadFromDataStore(ExtensionType extensionType, string extensionId);

        /// <summary>
        /// Loads the settings from the provider.
        /// </summary>
        /// <returns>A StringDictionary.</returns>
        public abstract StringDictionary LoadSettings();

        /// <summary>
        /// Removes settings from data store
        /// </summary>
        /// <param name="extensionType">
        /// Extension Type
        /// </param>
        /// <param name="extensionId">
        /// Extension Id
        /// </param>
        public abstract void RemoveFromDataStore(ExtensionType extensionType, string extensionId);

       

        /// <summary>
        /// Saves the settings to the provider.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public abstract void SaveSettings(StringDictionary settings);

        /// <summary>
        /// Saves settings to data store
        /// </summary>
        /// <param name="extensionType">
        /// Extension Type
        /// </param>
        /// <param name="extensionId">
        /// Extension Id
        /// </param>
        /// <param name="settings">
        /// Settings object
        /// </param>
        public abstract void SaveToDataStore(ExtensionType extensionType, string extensionId, object settings);
        */
        #endregion
    }

    /// <summary>
    /// A configuration section for web.config.
    /// </summary>
    /// <remarks>
    /// In the config section you can specify the provider you 
    ///     want to use for BlogEngine.NET.
    /// </remarks>
    public class BlogProviderSection : ConfigurationSection
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the name of the default provider
        /// </summary>
        [StringValidator(MinLength = 1)]
        [ConfigurationProperty("defaultProvider", DefaultValue = "XmlBlogProvider")]
        public string DefaultProvider
        {
            get
            {
                return (string)base["defaultProvider"];
            }

            set
            {
                base["defaultProvider"] = value;
            }
        }

        /// <summary>
        ///     Gets a collection of registered providers.
        /// </summary>
        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return (ProviderSettingsCollection)base["providers"];
            }
        }

        #endregion
    }

    /// <summary>
    /// A collection of all registered providers.
    /// </summary>
    public class BlogProviderCollection : ProviderCollection
    {
        #region Indexers

        /// <summary>
        ///     Gets a provider by its name.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        public new BlogProvider this[string name]
        {
            get
            {
                return (BlogProvider)base[name];
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a provider to the collection.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            if (!(provider is BlogProvider))
            {
                throw new ArgumentException("Invalid provider type", "provider");
            }

            base.Add(provider);
        }

        #endregion
    }
}