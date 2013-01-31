namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Web;

    /// <summary>
    /// Provides a type to describe an OData query operation.
    /// </summary>
    public class ODataQueryOperation
    {
        /// <summary>
        /// Gets the current <see cref="ODataQueryOperation"/> from the current <see cref="HttpContext"/>.
        /// </summary>
        public static ODataQueryOperation Current
        {
            get
            {
                if (HttpContext.Current != null)
                    return (ODataQueryOperation)HttpContext.Current.Items["ODataQueryOperation.Current"];

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the operation <see cref="Type"/>.
        /// </summary>
        public Type OfType { get; set; }

        /// <summary>
        /// Gets or sets the operation SkipCount value.
        /// </summary>
        public int SkipCount { get; set; }

        /// <summary>
        /// Gets or sets the operation TopCount value.
        /// </summary>
        public int TopCount { get; set; }

        /// <summary>
        /// Gets or sets the operation FilterExpression value.
        /// </summary>
        public Expression FilterExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is a count operation or not.
        /// </summary>
        public bool IsCountRequest { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataOrderExpression"/> stack for the operation.
        /// </summary>
        public Stack<ODataOrderExpression> OrderStack { get; set; }

        /// <summary>
        /// Gets or sets the projected properties for the operation.
        /// </summary>
        public IDictionary<string, PropertyDetails> ProjectedProperties { get; set; }

        /// <summary>
        /// Gets or sets the projection expression for the operation.
        /// </summary>
        public LambdaExpression ProjectionExpression { get; set; }

        /// <summary>
        /// Gets or sets the projected type for the operation.
        /// </summary>
        public Type ProjectedType { get; set; }

        /// <summary>
        /// Gets or sets the continuation token value for the operation.
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Gets or sets the operation's context parameters.
        /// </summary>
        public Dictionary<string, string> ContextParameters { get; set; }

        /// <summary>
        /// Determines whether a property is a projected property or not.
        /// </summary>
        /// <param name="propertyName">The property name to be evaluated.</param>
        /// <returns>A System.Boolean value that indicates whether the property is a projected property or not.</returns>
        public bool IsPropertyInProjection(string propertyName)
        {
            return this.ProjectionExpression == null ||
                   this.ProjectedProperties.ContainsKey(propertyName);
        }
    }
}