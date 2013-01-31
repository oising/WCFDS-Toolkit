namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;

    /// <summary>
    /// Specify whether the repository method will manage the specified set to true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RepositoryBehaviorAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the repository method will handle everything.
        /// </summary>
        public bool HandlesEverything { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the repository method will handle filters.
        /// </summary>
        public bool HandlesFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the repository method will handle order by.
        /// </summary>
        public bool HandlesOrderBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the repository method will handle select.
        /// </summary>
        public bool HandlesSelect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the repository method will handle skip.
        /// </summary>
        public bool HandlesSkip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the repository method will handle top.
        /// </summary>
        public bool HandlesTop { get; set; }
    }
}
