namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;

    /// <summary>
    /// Specifies whether the repository will handle the collection count by the '{Entity}Count' method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class CollectionCountAttribute : RepositoryOperationAttribute
    {
    }
}