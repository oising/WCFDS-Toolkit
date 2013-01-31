namespace Microsoft.Data.Services.Toolkit.ServiceModel
{
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Custom BodyWriter for creating JSONP Responses.
    /// </summary>
    internal class JsonBodyWriter : BodyWriter
    {
        private readonly string content;
        private readonly Encoding contentEncoding;

        /// <summary>
        /// Initializes a new instance of the JsonBodyWriter class.
        /// </summary>
        /// <param name="content">Content to be written.</param>
        internal JsonBodyWriter(string content)
            : this(content, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonBodyWriter class.
        /// </summary>
        /// <param name="content">Content to be written.</param>
        /// <param name="contentEncoding">Encoding to be used on the writing.</param>
        internal JsonBodyWriter(string content, Encoding contentEncoding) : base(false)
        {
            this.content = content;
            this.contentEncoding = contentEncoding;
        }

        /// <summary>
        /// Writes the given content to output writer.
        /// </summary>
        /// <param name="writer">Output writer instance.</param>
        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            var buffer = this.contentEncoding.GetBytes(this.content);

            writer.WriteStartElement("Binary");
            writer.WriteBase64(buffer, 0, buffer.Length);
            writer.WriteEndElement();
        }
    }
}
