namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System.Linq.Expressions;

    /// <summary>
    /// Represents an order expression including its method name.
    /// </summary>
    public class ODataOrderExpression : Expression
    {
        /// <summary>
        /// Gets or sets the order by method name.
        /// </summary>
        public string OrderMethodName { get; set; }

        /// <summary>
        /// Gets or sets the order by expression.
        /// </summary>
        public Expression Expression { get; set; }
    }
}