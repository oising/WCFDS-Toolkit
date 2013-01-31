namespace Microsoft.Data.Services.Toolkit
{
    using System;
    using System.Linq;
    using System.Web;

    /// <summary>
    /// Marks an ODataService as OutputCache aware.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EnableOutputCacheAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the EnableOutputCacheAttribute class.
        /// </summary>
        public EnableOutputCacheAttribute()
        {
            this.ExpiresInSeconds = 30;
            this.HttpCacheability = HttpCacheability.ServerAndPrivate;
        }

        /// <summary>
        /// Gets or sets a value indicating the expiration time in seconds.
        /// </summary>
        /// <remarks>By default it uses 30 seconds.</remarks>
        public int ExpiresInSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the HttpCacheability settings 
        /// for the current service.
        /// </summary>
        /// <remarks>By default it uses HttpCacheability.ServerAndPrivate</remarks> 
        public HttpCacheability HttpCacheability { get; set; }

        /// <summary>
        /// Looks for an OutputCacheSettingsAttribute instance from
        /// the given service type.
        /// </summary>
        /// <param name="serviceType">Type of the service to extract the 
        /// OuputCacheSettings instance from.</param>
        /// <returns>Returns an OutputCacheSettingsAttribute instance (null when not present).</returns>
        public static EnableOutputCacheAttribute GetServiceOutputCacheSettings(Type serviceType)
        {
            return serviceType.GetCustomAttributes(typeof(EnableOutputCacheAttribute), false)
                              .Cast<EnableOutputCacheAttribute>()
                              .SingleOrDefault();
        }
    }
}
