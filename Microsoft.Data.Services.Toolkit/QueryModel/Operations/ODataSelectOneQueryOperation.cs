namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides a type to describe an OData select one operation.
    /// </summary>
    public class ODataSelectOneQueryOperation : ODataQueryOperation
    {
        /// <summary>
        /// Gets or sets a parent operation for this operation.
        /// </summary>
        public ODataSelectOneQueryOperation Parent { get; set; }

        /// <summary>
        /// Gets or sets the key identifiers for this operation.
        /// </summary>
        public Dictionary<string, string> Keys { get; set; }

        /// <summary>
        /// Gets the first or default key identifier.
        /// </summary>
        public string Key
        {
            get
            {
                return (this.Keys ?? new Dictionary<string, string>(StringComparer.InvariantCulture)).Values.FirstOrDefault();
            }
        }
    }
}