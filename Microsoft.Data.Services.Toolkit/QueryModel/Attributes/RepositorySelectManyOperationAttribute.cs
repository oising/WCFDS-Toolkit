namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    /// <summary>
    /// Specifies the name of the parent and the alias for the navigation property 
    /// if you don't use the Get{Entity}By{Property} convention method.
    /// </summary>
    public abstract class RepositorySelectManyOperationAttribute : RepositoryOperationAttribute
    {
        /// <summary>
        /// Gets or sets the of parent alias.
        /// </summary>
        public string ParentAlias { get; set; }

        /// <summary>
        /// Gets or sets the name of property alias.
        /// </summary>
        public string PropertyAlias { get; set; }
    }
}