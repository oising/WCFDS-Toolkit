using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Providers;
using System.Linq;
using Microsoft.Data.Services.Toolkit.QueryModel;

namespace Microsoft.Data.Services.Toolkit.Providers
{
    public class AzureTablePagingProvider : GenericPagingProvider
    {
        public AzureTablePagingProvider(Dictionary<string, int> pageSizes, ODataContext context)
            : base(pageSizes, context)
        {
            context.SetPagingProvider(this);
        }

        public string NextPartition { get; private set; }
        public string NextRowKey { get; private set; }

        public DataServiceQuery<T> AddContinutationToQuery<T>(DataServiceQuery<T> query)
        {
            if (!string.IsNullOrWhiteSpace(NextPartition) &&
                !string.IsNullOrWhiteSpace(NextRowKey))
                return query.AddQueryOption("NextPartitionKey", NextPartition).AddQueryOption("NextRowKey", NextRowKey);

            return query;
        }

        public void RetreiveContinuationFromResponse(QueryOperationResponse response)
        {
            string nextPartition = null;
            string nextRowKey = null;
            response.Headers.TryGetValue("x-ms-continuation-NextPartitionKey", out nextPartition);
            response.Headers.TryGetValue("x-ms-continuation-NextRowKey", out nextRowKey);

            if (!string.IsNullOrWhiteSpace(nextPartition) &&
                !string.IsNullOrWhiteSpace(nextRowKey))
                SetToken(nextPartition, nextRowKey);
        }

        public IEnumerable<T> ExecuteQueryWithContinutation<T>(DataServiceQuery<T> query)
        {
            query = AddContinutationToQuery(query);

            var response = (QueryOperationResponse)query.Execute();

            this.RetreiveContinuationFromResponse(response);

            return (IEnumerable<T>)response;
        }

        public override object[] GetContinuationToken(IEnumerator enumerator)
        {
            // Grab the continuation token from the base provider, and if a token
            // wasn't returned, then immediately return null.
            var continuationToken = base.GetContinuationToken(enumerator);

            if (continuationToken == null)
                return null;

            // If the current paging provider has an Azure Table continuation token
            // then we need to return it, optionally with the continuation token provided
            // by the base provider.
            if (!string.IsNullOrWhiteSpace(NextPartition) &&
                !string.IsNullOrWhiteSpace(NextRowKey))
            {
                if (continuationToken == null)
                    return new object[] { NextPartition, NextRowKey };
                else
                    return new object[] { NextPartition, NextRowKey, continuationToken[0] };
            }

            return continuationToken;
        }

        public override void SetContinuationToken(IQueryable query, ResourceType resourceType, object[] continuationToken)
        {
            // If there is a continuation coming in from the request, that means that we'll
            // have both the Azure Tables continuation as well as the token used for SQL Azure.
            if (continuationToken != null)
            {
                // Parse out the Azure Tables continuation from the incoming token,
                // and then pass the SQL Azure token down to the base provider (this will provided
                // the skip/top data used by SQL Azure).
                NextPartition = continuationToken[0].ToString();
                NextRowKey = continuationToken[1].ToString();
                base.SetContinuationToken(query, resourceType, new object[] { continuationToken[2] });
            }
            else
            {
                // Since no continuation was set, we need to pass the token down to the base provider
                // so that is can set the appropriate skip/top data to handle server-driven paging.
                base.SetContinuationToken(query, resourceType, continuationToken);
            }
        }

        public void SetToken(string nextPartition, string nextRowKey)
        {
            NextPartition = nextPartition;
            NextRowKey = nextRowKey;
        }
    }
}