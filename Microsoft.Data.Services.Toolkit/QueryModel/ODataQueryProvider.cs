namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web;
    using System.ServiceModel.Web;
    using System.Collections.Specialized;

    /// <summary>
    /// A custom implementation of the <see cref="IQueryProvider"/> contract.
    /// </summary>
    /// <typeparam name="T">The query provider type.</typeparam>
    public class ODataQueryProvider<T> : IQueryProvider
    {
        private readonly Func<string, object> resolver;

        /// <summary>
        /// Initializes a new instance of the ODataQueryProvider class.
        /// </summary>
        /// <param name="resolver">A functional to resolve the repository.</param>
        public ODataQueryProvider(Func<string, object> resolver)
        {
            this.resolver = resolver;
        }

        /// <summary>
        /// Gets or sets the continuation token for provider.
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the paging is skip/take based or not.
        /// </summary>
        public bool SkipTakeBasedPaging { get; set; }

        /// <summary>
        /// Gets the context paramenters.
        /// </summary>
        public Dictionary<string, string> ContextParameters
        {
            get
            {
                if (HttpContext.Current == null)
                    return null;

                var queryString = HttpContext.Current.Request.QueryString;
                var headers = HttpContext.Current.Request.Headers;

                var queryStringDictionary = queryString.AllKeys.Where(p => !p.StartsWith("$")).ToDictionary(k => k, p => queryString[p]);

                foreach (var header in headers.AllKeys)
                    queryStringDictionary.Add(header, headers[header]);

                return queryStringDictionary;
            }
        }

        /// <summary>
        /// Creates a new <see cref="IQueryable"/> instance for a given <see cref="Expression"/>.
        /// </summary>
        /// <typeparam name="TElement">The type of the <see cref="IQueryable"/>.</typeparam>
        /// <param name="expression">The OData query <see cref="Expression"/>.</param>
        /// <returns>A new ODataQuery instance of the provided type.</returns>
        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new ODataQuery<TElement>(new ODataQueryProvider<TElement>(this.resolver), expression);
        }

        /// <summary>
        /// Creates a new <see cref="IQueryable"/> instance for a given <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The OData query <see cref="Expression"/>.</param>
        /// <returns>A new ODataQuery instance.</returns>
        public IQueryable CreateQuery(Expression expression)
        {
            try
            {
                return new ODataQuery<T>(this, expression);
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// Executes an OData <see cref="Expression"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="expression">The OData query <see cref="Expression"/>.</param>
        /// <returns>The result of the OData expression execution.</returns>
        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)this.Execute(expression);
        }

        /// <summary>
        /// Executes an OData <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The OData query <see cref="Expression"/>.</param>
        /// <returns>The result of the OData expression execution.</returns>
        public virtual object Execute(Expression expression)
        {
            var visitor = new ODataExpressionVisitor(expression);
            var operation = visitor.Eval();

            if (HttpContext.Current != null)
                HttpContext.Current.Items.Add("ODataQueryOperation.Current", operation);

            return this.ExecuteQuery(operation);
        }

        /// <summary>
        /// Executes an <see cref="ODataQueryOperation"/>.
        /// </summary>
        /// <param name="operation">The <see cref="ODataQueryOperation"/> to be executed..</param>
        /// <returns>The result of the OData expression execution.</returns>
        public virtual object ExecuteQuery(ODataQueryOperation operation)
        {
            try
            {
                if (operation == null)
                    throw new NotSupportedException();

                if (operation is ODataCompoundQueryOperation)
                {
                    var compoundQuery = operation as ODataCompoundQueryOperation;
                    var item = (this.ExecuteQuery(compoundQuery.AnonymousGetManyOperation) as IEnumerable<object>).First();

                    compoundQuery.OfType = item.GetType();
                    compoundQuery.Keys = item.DataServiceKeys();
                }

                operation.ContextParameters = this.ContextParameters;

                if (!this.SkipTakeBasedPaging)
                    operation.ContinuationToken = this.ContinuationToken;
                else
                {
                    var parts = this.ContinuationToken.Split(':').Select(int.Parse);
                    operation.SkipCount = parts.First();
                    operation.TopCount = parts.Last();
                }

                var repository = ODataOperationResolver.For(operation).Repository(this.resolver);
                var method = ODataOperationResolver.For(operation).Method(repository);
                var arguments = ODataOperationResolver.For(operation).Arguments(method);
                var temp = method.Invoke(repository, arguments);

                if (temp == null)
                    return null;

                if (temp.GetType() == typeof(long) && operation.IsCountRequest && operation.OfType.GetAttribute<CollectionCountAttribute>() != null)
                    return temp;

                var manyOperation = operation as ODataSelectManyQueryOperation;

                if (temp.GetType() == typeof(long) && operation.IsCountRequest && manyOperation.NavigationPropertyInfo.GetAttribute<ForeignPropertyCountAttribute>() != null)
                    return temp;

                if (manyOperation != null && manyOperation.NavigationPropertyInfo.GetAttribute<ForeignPropertyAttribute>() == null)
                    temp = manyOperation.NavigationPropertyInfo.GetValue(temp, null);

                var elementType = manyOperation == null ? operation.OfType : manyOperation.NavigationPropertyType;
                var result = (IEnumerable<object>)ODataOperationExecutionHelper.EvalEnumerable(temp.WrapIntoEnumerable(), elementType);

                var behavior = repository.GetBehavior(method);

                if (!behavior.HandlesEverything)
                {
                    if (!behavior.HandlesFilter && operation.FilterExpression != null)
                        result = (IEnumerable<object>)ODataOperationExecutionHelper.ExecuteLinq2ObjectsImplementation("Where", result, operation.FilterExpression);

                    while (!behavior.HandlesOrderBy && operation.OrderStack != null && operation.OrderStack.Count > 0)
                    {
                        var orderExpression = operation.OrderStack.Pop();
                        result = (IEnumerable<object>)ODataOperationExecutionHelper.ExecuteLinq2ObjectsImplementation(orderExpression.OrderMethodName, result, orderExpression.Expression);
                    }

                    if (!behavior.HandlesSkip && operation.SkipCount > 0)
                        result = (IEnumerable<object>)ODataOperationExecutionHelper.ExecuteLinq2ObjectsImplementation("Skip", result, operation.SkipCount);

                    if (!behavior.HandlesTop && operation.TopCount > 0)
                        result = (IEnumerable<object>)ODataOperationExecutionHelper.ExecuteLinq2ObjectsImplementation("Take", result, operation.TopCount);
                }

                if (operation.IsCountRequest)
                    return result.LongCount();

                if (operation.ProjectionExpression != null)
                    result = (IEnumerable<object>)ODataOperationExecutionHelper.ExecuteProjection(operation, result);

                return result;
            }
            catch (Exception ex)
            {
                var realException = ex is TargetInvocationException ? ex.InnerException : ex;

                if (!(realException is DataServiceException))
                    realException = new DataServiceException(500, "Internal Server Error", realException.Message, "en-US", realException);

                throw realException;
            }
        }
    }
}