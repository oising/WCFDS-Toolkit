namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;

    /// <summary>
    /// Specifies the source property name for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SourcePropertyNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the SourcePropertyNameAttribute class.
        /// </summary>
        /// <param name="underlyingName">A string containing the underplaying name.</param>
        public SourcePropertyNameAttribute(string underlyingName)
        {
            this.UnderlyingName = underlyingName;
        }
        
        internal SourcePropertyNameAttribute()
        {
        }

        /// <summary>
        /// Gets or sets the underplaying name.
        /// </summary>
        public string UnderlyingName { get; set; }
    }
}