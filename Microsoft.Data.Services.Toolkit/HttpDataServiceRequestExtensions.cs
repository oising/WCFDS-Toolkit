namespace Microsoft.Data.Services.Toolkit
{
    using System.Linq;
    using System.Web;

    /// <summary>
    /// Extension methods for the <see cref="HttpRequest"/> class.
    /// </summary>
    public static class HttpDataServiceRequestExtensions
    {
        /// <summary>
        /// Returns whether the client is requesting service metadata or not. 
        /// </summary>
        /// <param name="request">The current http request.</param>
        /// <returns>A value indicating whether the request is requesting service metadata or not.</returns>
        public static bool IsMetadataRequest(this HttpRequest request)
        {
            return request.Url.Segments.Last().Equals("$metadata");
        }

        /// <summary>
        /// Returns whether the client is requesting the root collection list.
        /// </summary>
        /// <param name="request">The current http request.</param>
        /// <returns>A value indicating whether the client is requesting the root level collection list or not.</returns>
        public static bool IsRootCollectionListRequest(this HttpRequest request)
        {
            var root = request.Path.Replace(request.ApplicationPath, string.Empty).Replace("//", "/");
            return string.IsNullOrEmpty(root) || root.Equals("/");
        }
    }
}
