namespace Microsoft.Data.Services.Toolkit
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Sanitizes url characters.
    /// </summary>
    public class UrlSanitizer
    {
        private static readonly MethodInfo GetSyntaxMethod = typeof(UriParser).GetMethod("GetSyntax", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly FieldInfo FlagsField = typeof(UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly UrlSettingsAttribute urlSettings;

        /// <summary>
        /// Initializes a new instance of the UrlSanitizer class.
        /// </summary>
        /// <param name="urlSettings">The url settings for the sanitization process.</param>
        public UrlSanitizer(UrlSettingsAttribute urlSettings)
        {
            this.urlSettings = urlSettings;
        }

        /// <summary>
        /// Enables OAuth support.
        /// </summary>
        public static void EnableOAuthSupport()
        {
            foreach (var scheme in new[] { "http", "https" })
            {
                var parser = (UriParser)GetSyntaxMethod.Invoke(null, new object[] { scheme });

                if (parser == null) 
                    continue;

                var flagsValue = (int)FlagsField.GetValue(parser);
                if ((flagsValue & 0x1000000) != 0)
                    FlagsField.SetValue(parser, flagsValue & ~0x1000000);
            }
        }

        /// <summary>
        /// Executes the Sanitization process.
        /// </summary>
        /// <param name="uri">The Uri to be sanitized.</param>
        /// <returns>The sanitized <see cref="Uri"/>.</returns>
        public Uri Sanitize(string uri)
        {
            var replaceUri = new StringBuilder();
            var inQuote = false;

            var escapedCharacters = this.urlSettings.GetEscapedCharacters();

            foreach (var t in uri)
            {
                switch (t)
                {
                    case '\'':
                        replaceUri.Append(t);
                        inQuote = !inQuote;
                        break;
                    case '#':
                    case '\\':
                    case '/':
                    case '?':
                    case '&':
                        if (inQuote && escapedCharacters.Contains(t))
                        {
                            replaceUri.AppendFormat("%{0:X}", (int)t);
                        }
                        else
                            replaceUri.Append(t);
                        break;
                    default:
                        replaceUri.Append(t);
                        break;
                }
            }

            return new Uri(replaceUri.ToString());
        }
    }
}
