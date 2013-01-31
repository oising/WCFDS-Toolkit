namespace Microsoft.Data.Services.Toolkit.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Providers;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using QueryModel;

    /// <summary>
    /// A generic implementation for the <see cref="IDataServicePagingProvider"/> contract.
    /// </summary>
    public class GenericPagingProvider : IDataServicePagingProvider
    {
        private readonly Dictionary<string, int> pageSizes;
        private readonly object currentDataSource;

        private int? lastReceivedPage;
        private string entityTypeName;
        private Expression expression;
        private IQueryProvider queryProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericPagingProvider"/> class.
        /// </summary>
        /// <param name="pageSizes">The page size for each Data Service entity.</param>
        /// <param name="currentDataSource">The current data source object.</param>
        public GenericPagingProvider(Dictionary<string, int> pageSizes, object currentDataSource)
        {
            this.pageSizes = pageSizes;
            this.currentDataSource = currentDataSource;
        }

        /// <summary>
        /// Returns the next-page token to put in the $skiptoken query option.
        /// </summary>
        /// <param name="enumerator">Enumerator for which the continuation token is being requested.</param>
        /// <returns>The next-page token as a collection of primitive types.</returns>
        public virtual object[] GetContinuationToken(IEnumerator enumerator)
        {
            if (this.lastReceivedPage == null)
                return null;

            var operation = new ODataExpressionVisitor(this.expression).Eval();
            var compound = operation as ODataCompoundQueryOperation;

            if (compound != null)
            {
                var partial = (this.queryProvider.GetType().InvokeMember("ExecuteQuery", BindingFlags.InvokeMethod, null, this.queryProvider, new object[] { compound.AnonymousGetManyOperation }) as IEnumerable<object>).First();

                compound.OfType = partial.GetType().TypeOrElementType();
                compound.Keys = partial.DataServiceKeys();
            }

            if (operation != null)
            {
                operation.TopCount = 0;
                operation.SkipCount = 0;
                operation.ContinuationToken = null;
                operation.IsCountRequest = true;

                this.queryProvider.GetType().GetProperty("SkipTakeBasedPaging").SetValue(this.queryProvider, false, null);

                var @return = operation is ODataSelectManyQueryOperation ? (operation as ODataSelectManyQueryOperation).NavigationPropertyType : operation.OfType;
                var queryProviderType = typeof(ODataQueryProvider<>).MakeGenericType(@return);
                var result = (long)queryProviderType.InvokeMember("ExecuteQuery", BindingFlags.InvokeMethod, null, this.queryProvider, new object[] { operation });

                if ((this.lastReceivedPage * this.PageSizeFor(@return)) + this.CurrentOffset() >= result)
                    return null;

                var token = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", this.entityTypeName, this.lastReceivedPage + 1);

                return new object[] { Convert.ToBase64String(Encoding.Default.GetBytes(token)) };
            }

            return null;
        }

        /// <summary>
        /// Gets the next-page token from the $skiptoken query option in the request URI.
        /// </summary>
        /// <param name="query">Query for which the continuation token is being provided.</param>
        /// <param name="resourceType">Resource type of the result on which the $skip token is to be applied.</param>
        /// <param name="continuationToken">Continuation token parsed into primitive type values.</param>
        public virtual void SetContinuationToken(IQueryable query, ResourceType resourceType, object[] continuationToken)
        {
            if (IsTopRequest())
                return;

            var instanceType = resourceType.InstanceType;
            var queryType = typeof(ODataQuery<>).MakeGenericType(instanceType);
            this.expression = query.Expression;
            this.queryProvider = query.Provider;

            if (queryType.IsAssignableFrom(query.GetType()))
            {
                if (this.SupportsType(instanceType))
                {
                    if (continuationToken != null && continuationToken[0] != null)
                    {
                        var token = Encoding.Default.GetString(Convert.FromBase64String(continuationToken[0].ToString()));
                        var tokenParts = token.Split(':');
                        this.entityTypeName = tokenParts[0];
                        this.lastReceivedPage = Convert.ToInt32(tokenParts[1]);
                    }
                    else
                    {
                        this.entityTypeName = instanceType.Name;
                        this.lastReceivedPage = 1;
                    }

                    var provider = queryType.GetProperty("Provider").GetValue(query, null);
                    var skip = (this.lastReceivedPage * this.PageSizeFor(instanceType)) - this.PageSizeFor(instanceType);
                    var nextContinuationToken = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", skip + this.CurrentOffset(), this.PageSizeFor(instanceType));
                    provider.GetType().GetProperty("ContinuationToken").SetValue(provider, nextContinuationToken, null);
                    provider.GetType().GetProperty("SkipTakeBasedPaging").SetValue(provider, true, null);
                }
            }
        }

        /// <summary>
        /// Determines whether is a top request or not based on the query string parameters.
        /// </summary>
        /// <returns>A System.Boolean value that indicates whether $top parameter was provided or not.</returns>
        protected static bool IsTopRequest()
        {
            return HttpContext.Current.Request.QueryString.AllKeys.Contains("$top");
        }

        /// <summary>
        /// Determines if entity's page size is defined in Data Service configuration.
        /// </summary>
        /// <param name="type">The entity type.</param>
        /// <returns>A System.Boolean value that indicates whether the entity's page size was set or not.</returns>
        protected bool SupportsType(Type type)
        {
            var queryableType = typeof(IQueryable<>).MakeGenericType(type);
            var property = this.currentDataSource.GetType().GetProperties().Where(p => p.PropertyType.IsAssignableFrom(queryableType)).FirstOrDefault();

            return property != null && (this.pageSizes.ContainsKey(property.Name) || this.pageSizes.ContainsKey("*"));
        }

        /// <summary>
        /// Gets the page size for a given entity type from Data Service configuration.
        /// </summary>
        /// <param name="type">The entity type.</param>
        /// <returns>The page size for the given entity.</returns>
        protected int PageSizeFor(Type type)
        {
            var queryableType = typeof(IQueryable<>).MakeGenericType(type);
            var property = this.currentDataSource.GetType().GetProperties().Where(p => p.PropertyType.IsAssignableFrom(queryableType)).FirstOrDefault();

            return !this.pageSizes.ContainsKey(property.Name) ? this.pageSizes["*"] : this.pageSizes[property.Name];
        }

        /// <summary>
        /// Gets the $skip parameter from the query string.
        /// </summary>
        /// <returns>The skip value provided as part of the query string.</returns>
        protected int CurrentOffset()
        {
            return int.Parse(HttpContext.Current.Request.QueryString["$skip"] ?? "0");
        }
    }
}
