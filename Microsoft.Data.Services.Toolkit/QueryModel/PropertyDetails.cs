namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Describes the projection plan for a property.
    /// </summary>
    public class PropertyDetails
    {
        /// <summary>
        /// Gets or sets the actual property reference.
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// Gets or sets the delegate to be executed while setting the property.
        /// </summary>
        public Action<object, object> PropertySetter { get; set; }
    }
}