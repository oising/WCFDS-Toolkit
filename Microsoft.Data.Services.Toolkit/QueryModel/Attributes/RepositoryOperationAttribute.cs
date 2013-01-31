namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;

    /// <summary>
    /// Specifies the name of the repository method called by the runtime
    /// instead of using naming conventions.
    /// </summary>
    public abstract class RepositoryOperationAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating the name of the repository method.
        /// </summary>
        public string RepositoryMethod { get; set; }
    }
}