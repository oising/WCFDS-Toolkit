namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// A custom implementation of the <see cref="IOrderedQueryable"/> contract.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the ODataQuery.</typeparam>
    public class ODataQuery<T> : IOrderedQueryable<T>
    {
        private readonly IQueryProvider provider;
        private readonly Expression expression;

        /// <summary>
        /// Initializes a new instance of the ODataQuery class.
        /// </summary>
        /// <param name="provider">A provider that implements the <see cref="IQueryProvider"/> contract.</param>
        public ODataQuery(ODataQueryProvider<T> provider)
        {
            this.provider = provider;
            this.expression = Expression.Constant(this);
        }

        /// <summary>
        /// Initializes a new instance of the ODataQuery class.
        /// </summary>
        /// <param name="provider">A provider that implements the <see cref="IQueryProvider"/> contract.</param>
        /// <param name="expression">An OData <see cref="Expression"/>.</param>
        public ODataQuery(ODataQueryProvider<T> provider, Expression expression)
            : this(provider)
        {
            this.expression = expression;
        }

        /// <summary>
        /// Gets the element type of the current ODataQuery.
        /// </summary>
        public Type ElementType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Gets the OData query <see cref="Expression"/>.
        /// </summary>
        public Expression Expression
        {
            get { return this.expression; }
        }

        /// <summary>
        /// Gets the current <see cref="IQueryProvider"/> implementation.
        /// </summary>
        public IQueryProvider Provider
        {
            get
            {
                return this.provider;
            }
        }

        /// <summary>
        /// Executes the provided OData query expression.
        /// </summary>
        /// <returns>The result of the OData query expression execution.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            var result = this.Provider.Execute<IEnumerable<T>>(this.expression);
            return (result ?? new List<T>()).GetEnumerator();
        }

        /// <summary>
        /// Executes the provided OData query expression.
        /// </summary>
        /// <returns>The result of the OData query expression execution.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}