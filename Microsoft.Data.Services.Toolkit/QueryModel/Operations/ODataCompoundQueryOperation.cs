namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    /// <summary>
    /// Provides a type to describe an OData compound query operation.
    /// </summary>
    public class ODataCompoundQueryOperation : ODataSelectManyQueryOperation
    {
        /// <summary>
        /// Gets or sets the <see cref="ODataSelectManyQueryOperation"/> for a compound query.
        /// </summary>
        /// <value>The anonymous get many operation.</value>
        public ODataSelectManyQueryOperation AnonymousGetManyOperation { get; set; }
    }
}