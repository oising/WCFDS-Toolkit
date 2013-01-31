namespace Microsoft.Data.Services.Toolkit.Providers
{
    using System;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Provides an easy way to return streamed content from an Uri.
    /// </summary>
    public class EntityUrlReadOnlyStreamProvider : IDataServiceStreamProvider
    {
        /// <summary>
        /// Gets the stream buffer size.
        /// </summary>
        public virtual int StreamBufferSize
        {
            get { return 64000; }
        }

        /// <summary>
        /// Returns the default stream that is associated with an entity that has a binary property.
        /// </summary>
        /// <param name="entity">The entity for which the descriptor object should be returned.</param>
        /// <param name="etag">The eTag value sent as part of the HTTP request that is sent to the data service.</param>
        /// <param name="checkETagForEquality">A nullable System.Boolean value that determines the type of eTag that is used.</param>
        /// <param name="operationContext">The System.Data.Services.DataServiceOperationContext instance that processes the request.</param>
        /// <returns>The data System.IO.Stream that contains the binary property data of the entity.</returns>
        public virtual Stream GetReadStream(object entity, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext)
        {
            var streameable = entity as IStreamEntity;

            if (streameable == null)
                return null;

            using (var client = new WebClient())
            {
                var bytes = client.DownloadData(streameable.GetUrlForStreaming());
                return new MemoryStream(bytes);
            }
        }
        
        /// <summary>
        /// Returns the URI that is used to request the data stream that is associated with the binary property of an entity.
        /// </summary>
        /// <param name="entity">The entity that has the associated binary data stream.</param>
        /// <param name="operationContext">The System.Data.Services.DataServiceOperationContext 
        /// instance used by the data service to process the request.</param>
        /// <returns>A System.Uri value that is used to request the binary data stream.</returns>
        public virtual Uri GetReadStreamUri(object entity, DataServiceOperationContext operationContext)
        {
            var streameable = entity as IStreamEntity;

            return streameable != null ? streameable.GetUrlForStreaming() : null;
        }
        
        /// <summary>
        /// Returns the content type of the stream that is associated with the specified entity.
        /// </summary>
        /// <param name="entity">The entity that has the associated binary data stream.</param>
        /// <param name="operationContext">The System.Data.Services.DataServiceOperationContext 
        /// instance used by the data service to process the request.</param>
        /// <returns>A valid Content-Type of the binary data.</returns>
        public virtual string GetStreamContentType(object entity, DataServiceOperationContext operationContext)
        {
            var streameable = entity as IStreamEntity;

            return streameable != null ? streameable.GetContentTypeForStreaming() : null;
        }

        /// <summary>
        /// Returns the eTag of the data stream that is associated with the specified entity.
        /// </summary>
        /// <param name="entity">The entity that has the associated binary data stream.</param>
        /// <param name="operationContext">The System.Data.Services.DataServiceOperationContext 
        /// instance used by the data service to process the request.</param>
        /// <returns>ETag of the stream associated with the entity.</returns>
        public virtual string GetStreamETag(object entity, DataServiceOperationContext operationContext)
        {
            var streameable = entity as IStreamEntity;

            return streameable != null ? streameable.GetStreamETag() : null;
        }

        /// <summary>
        /// Returns the stream that the data service uses to write the contents of a binary property that is associated with an entity.
        /// </summary>
        /// <param name="entity">The entity that has the associated binary stream.</param>
        /// <param name="etag">The eTag value that is sent as part of the HTTP request that is sent to the data service.</param>
        /// <param name="checkETagForEquality">A nullable System.Boolean value that determines the type of eTag is used.</param>
        /// <param name="operationContext">The System.Data.Services.DataServiceOperationContext 
        /// instance that is used by the data service to process the request.</param>
        /// <returns>A valid System.Stream the data service uses to write the contents of a 
        /// binary property that is associated with the entity.</returns>
        public virtual Stream GetWriteStream(object entity, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is invoked by the data services framework to obtain metadata about the stream associated with the specified entity.
        /// </summary>
        /// <param name="entity">The entity for which the descriptor object should be returned.</param>
        /// <param name="operationContext">The System.Data.Services.DataServiceOperationContext instance that processes the request.</param>
        public virtual void DeleteStream(object entity, DataServiceOperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a namespace-qualified type name that represents the type that the data service 
        /// runtime must create for the Media Link Entry that is associated with the data stream 
        /// for the Media Resource that is being inserted.
        /// </summary>
        /// <param name="entitySetName">Fully-qualified entity set name.</param>
        /// <param name="operationContext">The System.Data.Services.DataServiceOperationContext 
        /// instance that is used by the data service to process the request.</param>
        /// <returns>A namespace-qualified type name.</returns>
        public virtual string ResolveType(string entitySetName, DataServiceOperationContext operationContext)
        {
            throw new NotImplementedException();
        }
    }
}
