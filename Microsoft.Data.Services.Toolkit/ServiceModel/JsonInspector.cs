namespace Microsoft.Data.Services.Toolkit.ServiceModel
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Text;

    /// <summary>
    /// Inspects incoming Requests for JSONP request detection,
    /// when parses the input and generates the proper output 
    /// for JSONP compliance.
    /// </summary>
    internal class JsonInspector : IDispatchMessageInspector
    {
        /// <summary>
        /// Determines whether the requested message is JSONP. When detected
        /// changes the message to be treated by the WCF Data services Runtime 
        /// as a common JSON request and correlates the output with the requested
        /// callback.
        /// </summary>
        /// <param name="request">Requested message.</param>
        /// <param name="channel">Current client channel.</param>
        /// <param name="instanceContext">Context where the service is running.</param>
        /// <returns>Returns a correlation value indicating the requested callback (when applies).</returns>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (request.Properties.ContainsKey("UriTemplateMatchResults"))
            {
                var match = (UriTemplateMatch)request.Properties["UriTemplateMatchResults"];
                var format = match.QueryParameters["$format"];

                if ("json".Equals(format, StringComparison.InvariantCultureIgnoreCase))
                {
                    // strip out $format from the query options to avoid an error
                    // due to use of a reserved option (starts with "$")
                    match.QueryParameters.Remove("$format");

                    // replace the Accept header so that the Data Services runtime 
                    // assumes the client asked for a JSON representation
                    var httpmsg = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
                    httpmsg.Headers["Accept"] = "application/json";

                    var callback = match.QueryParameters["$callback"];

                    if (!string.IsNullOrEmpty(callback))
                    {
                        match.QueryParameters.Remove("$callback");
                        return callback;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Wraps the resulting content into a JSONP callback function 
        /// extracted on the AfterReceiveReply message.
        /// </summary>
        /// <param name="reply">Outgoing response message.</param>
        /// <param name="correlationState">Correlation state returned by the AfterReceiveReply method.</param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (correlationState == null || !(correlationState is string)) 
                return;

            // If we have a JSONP callback then buffer the response, wrap it with the
            // callback call and then re-create the response message
            var callback = (string)correlationState;

            var reader = reply.GetReaderAtBodyContents();
            reader.ReadStartElement();

            var content = Encoding.UTF8.GetString(reader.ReadContentAsBase64());
            content = string.Format(CultureInfo.InvariantCulture, "{0}({1});", callback, content);

            var newReply = Message.CreateMessage(MessageVersion.None, string.Empty, new JsonBodyWriter(content));
            newReply.Properties.CopyProperties(reply.Properties);
            reply = newReply;

            // change response content type to text/javascript if the JSON (only done when wrapped in a callback)
            var replyProperties = 
                (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];

            replyProperties.Headers["Content-Type"] = 
                replyProperties.Headers["Content-Type"].Replace("application/json", "text/javascript");
        }
    }
}