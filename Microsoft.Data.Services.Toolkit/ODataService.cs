using System.Diagnostics;

namespace Microsoft.Data.Services.Toolkit
{
    using System;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Web;
    using Providers;
    using ServiceModel;

    /// <summary>
    /// A custom DataService implementation.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the data service context.</typeparam>
    [DispatchInspector(typeof(JsonInspector))]
    public class ODataService<T> : DataService<T>, IServiceProvider
    {
        /// <summary>
        /// Initializes a new instance of the ODataService class.
        /// </summary>
        public ODataService()
        {
            Debug.Assert(WebOperationContext.Current != null, "WebOperationContext.Current != null");

            var uri = OperationContext.Current.IncomingMessageProperties.ContainsKey("MicrosoftDataServicesRequestUri") 
                ? OperationContext.Current.IncomingMessageProperties["MicrosoftDataServicesRequestUri"] as Uri : HttpContext.Current.Request.Url;
            
            var rootUrl = OperationContext.Current.IncomingMessageProperties.ContainsKey("MicrosoftDataServicesRootUri") 
                ? OperationContext.Current.IncomingMessageProperties["MicrosoftDataServicesRootUri"] as Uri : null;

            var urlSettings = UrlSettingsAttribute.GetServiceUrlSettings(GetType());
            var urlSanitizer = new UrlSanitizer(urlSettings);
            
            Debug.Assert(uri != null, "uri != null");
            var sanitizedUrl = urlSanitizer.Sanitize(uri.ToString());

            // FIX: accessing the root causes a null reference error
            // http://wcfdstoolkit.codeplex.com/workitem/28
            if (rootUrl == null && WebOperationContext.Current.IncomingRequest.UriTemplateMatch == null)
            {
                rootUrl = uri;
            }

            var rootUri = rootUrl == null ? WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri : new Uri(rootUrl.ToString());
            var rootUriBuilder = new UriBuilder(rootUri);
            var requestUriBuilder = new UriBuilder(sanitizedUrl.ToString());

            //// If we're not debugging, then attempt to refactor
            //// the request host and port based on the URL settings.
            if (!HttpContext.Current.Request.IsLocal)
            {
                if (!string.IsNullOrWhiteSpace(urlSettings.Host))
                {
                    rootUriBuilder.Host = urlSettings.Host;
                    requestUriBuilder.Host = urlSettings.Host;
                }

                if (urlSettings.ClearPorts)
                {
                    rootUriBuilder.Port = -1;
                    requestUriBuilder.Port = -1;
                }
            }

            if (!rootUriBuilder.Path.EndsWith("/"))
                rootUriBuilder.Path = rootUriBuilder.Path += "/";

            if (!requestUriBuilder.Path.EndsWith("/"))
                requestUriBuilder.Path = requestUriBuilder.Path += "/";

            OperationContext.Current.IncomingMessageProperties["MicrosoftDataServicesRootUri"] = rootUriBuilder.Uri;
            OperationContext.Current.IncomingMessageProperties["MicrosoftDataServicesRequestUri"] = requestUriBuilder.Uri;
        }

        /// <summary>
        /// Returns the corresponding implementation based on the service type.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <returns>An instance of the service implementation.</returns>
        public virtual object GetService(Type serviceType)
        {
            if (serviceType == typeof(IDataServicePagingProvider))
                return new GenericPagingProvider(this.DataServiceConfiguration().PageSizes(), this.CurrentDataSource);

            return serviceType == typeof(IDataServiceStreamProvider) ? new EntityUrlReadOnlyStreamProvider() : null;
        }

        /// <summary>
        /// Configures the current <see cref="HttpContext"/> cache if it is provided.
        /// </summary>
        /// <param name="args">A <see cref="ProcessRequestArgs"/> containing the arguments to be processed.</param>
        protected override void OnStartProcessingRequest(ProcessRequestArgs args)
        {
            base.OnStartProcessingRequest(args);

            var cacheSettings = EnableOutputCacheAttribute.GetServiceOutputCacheSettings(GetType());
            if (cacheSettings == null)
                return;

            var context = HttpContext.Current;
            var cache = HttpContext.Current.Response.Cache;

            cache.SetCacheability(cacheSettings.HttpCacheability);
            cache.SetExpires(context.Timestamp.AddSeconds(cacheSettings.ExpiresInSeconds));

            cache.VaryByHeaders["Accept"] = true;
            cache.VaryByHeaders["Accept-Charset"] = true;
            cache.VaryByHeaders["Accept-Encoding"] = true;
            cache.VaryByParams["*"] = true;
        }

        public T DataSource
        {
            get { return CurrentDataSource; }
        }
    }
}
