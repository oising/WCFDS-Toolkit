namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;

    /// <summary>
    /// Specifies whether a property is a Navigation Property allowing the repository to handle 'Get{Property}By{Entity}'
    /// method to retrieve the items related to the entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ForeignPropertyAttribute : RepositorySelectManyOperationAttribute
    {
    }
}