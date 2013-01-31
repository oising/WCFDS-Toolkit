namespace Microsoft.Data.Services.Toolkit
{
    using System;
    using System.Linq;

    /// <summary>
    /// Specifies the url settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UrlSettingsAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlSettingsAttribute"/> class.
        /// </summary>
        public UrlSettingsAttribute()
        {
            this.ClearPorts = true;
            this.EscapedCharacters = @"\,/,#,?,&";
        }

        /// <summary>
        /// Gets or sets whether any ports contained within the requesting URI should be removed. The default is true.
        /// </summary>
        public bool ClearPorts { get; set; }

        /// <summary>
        /// Gets or sets the supported url escaped characters.
        /// </summary>
        public string EscapedCharacters { get; set; }

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Retrieves the service url settings.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <returns>The <see cref="UrlSettingsAttribute"/> value.</returns>
        public static UrlSettingsAttribute GetServiceUrlSettings(Type serviceType)
        {
            var attribute = serviceType.GetCustomAttributes(typeof(UrlSettingsAttribute), false).Cast<UrlSettingsAttribute>().SingleOrDefault();
            return attribute ?? new UrlSettingsAttribute();
        }

        internal char[] GetEscapedCharacters()
        {
            return this.EscapedCharacters.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c[0]).ToArray();
        }
    }
}
