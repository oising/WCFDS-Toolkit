using Microsoft.Data.Services.Toolkit.Providers;

namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    public interface IPageableDataContext
    {
        void SetPagingProvider(GenericPagingProvider pagingProvider);
    }
}