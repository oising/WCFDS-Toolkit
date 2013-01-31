namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Provides a type to describe an OData select many operation.
    /// </summary>
    public class ODataSelectManyQueryOperation : ODataSelectOneQueryOperation
    {
        /// <summary>
        /// Gets or sets the navigation property name.
        /// </summary>
        public string NavigationProperty { get; set; }

        /// <summary>
        /// Gets the navigation property <see cref="Type"/>.
        /// </summary>
        public Type NavigationPropertyType
        {
            get
            {
                return this.NavigationPropertyInfo.PropertyType.TypeOrElementType();
            }
        }

        /// <summary>
        /// Gets the navigation property <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo NavigationPropertyInfo
        {
            get
            {
                return this.OfType.GetProperty(this.NavigationProperty);
            }
        }
    }
}