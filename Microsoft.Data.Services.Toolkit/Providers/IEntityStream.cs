namespace Microsoft.Data.Services.Toolkit.Providers
{
    using System;

    /// <summary>
    /// Defines the contract for streaming content for entities with streamed content.
    /// </summary>
    public interface IStreamEntity
    {
        /// <summary>
        /// Calculates the content type for the streaming content.
        /// </summary>
        /// <returns>The streaming's content type.</returns>
        string GetContentTypeForStreaming();

        /// <summary>
        /// Decides the <see cref="Uri"/> from where the content will be retrieved.
        /// </summary>
        /// <returns>An <see cref="Uri"/> that indicates where the content will be retrieved.</returns>
        Uri GetUrlForStreaming();

        /// <summary>
        /// Calculates the current stream etag.
        /// </summary>
        /// <returns>The current stream etag value.</returns>
        string GetStreamETag();
    }
}