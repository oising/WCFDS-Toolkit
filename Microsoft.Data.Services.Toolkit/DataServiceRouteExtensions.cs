namespace Microsoft.Data.Services.Toolkit
{
    using System;
    using System.Data.Services;
    using System.ServiceModel.Activation;
    using System.Web.Routing;

    /// <summary>
    /// Add DataService routes extensions to the RouteCollection type.
    /// </summary>
    public static class DataServiceRouteExtensions
    {
        /// <summary>
        /// Enables the RouteCollection to add Data Services.
        /// </summary>
        /// <typeparam name="TService">Data Service type implementation.</typeparam>
        /// <param name="routes">RouteCollection currently being extended.</param>
        public static void AddDataServiceRoute<TService>(this RouteCollection routes)
        {
            routes.AddDataServiceRoute(typeof(TService), string.Empty);
        }

        /// <summary>
        /// Enables the RouteCollection to add Data Services.
        /// </summary>
        /// <param name="routes">RouteCollection currently being extended.</param>
        /// <param name="dataServiceType">Data Service type implementation.</param>
        public static void AddDataServiceRoute(this RouteCollection routes, Type dataServiceType)
        {
            routes.Add(new ServiceRoute(string.Empty, new DataServiceHostFactory(), dataServiceType));
        }

        /// <summary>
        /// Enables the RouteCollection to add Data Services.
        /// </summary>
        /// <param name="routes">RouteCollection currently being extended.</param>
        /// <param name="dataServiceType">Data Service type implementation.</param>
        /// <param name="prefix">Prefix to be used as service root.</param>
        public static void AddDataServiceRoute(this RouteCollection routes, Type dataServiceType, string prefix)
        {
            routes.Add(new ServiceRoute(prefix, new DataServiceHostFactory(), dataServiceType));
        }

        /// <summary>
        /// Enables the RouteCollection to add Data Services.
        /// </summary>
        /// <typeparam name="TService">Data Service type implementation.</typeparam>
        /// <param name="routes">RouteCollection currently being extended.</param>
        /// /// <param name="prefix">Prefix to be used as service root.</param>
        public static void AddDataServiceRoute<TService>(this RouteCollection routes, string prefix)
        {
            routes.AddDataServiceRoute(typeof(TService), prefix);
        }
    }
}
