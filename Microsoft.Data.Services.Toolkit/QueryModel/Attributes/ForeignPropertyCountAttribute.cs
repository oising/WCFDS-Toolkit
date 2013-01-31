namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;

    /// <summary>
    /// Specifies whether a property is a Navigation Property allowing the repository to handle 'Count{Property}By{Entity}' 
    /// method to count items related to the current entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ForeignPropertyCountAttribute : RepositorySelectManyOperationAttribute
    {
    }
}