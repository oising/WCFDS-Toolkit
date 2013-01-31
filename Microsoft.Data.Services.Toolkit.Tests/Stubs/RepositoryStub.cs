namespace Microsoft.Data.Services.Toolkit.Tests.Stubs
{
    using System;
    using Microsoft.Data.Services.Toolkit.QueryModel;

    internal class RepositoryStub
    {
        public object GetAll(ODataQueryOperation operation)
        {
            throw new NotImplementedException();
        }

        public object GetOne(ODataSelectOneQueryOperation operation)
        {
            throw new NotImplementedException();
        }

        public object MockMethodForDecoratedNavigationProperty(ODataSelectManyQueryOperation operation)
        {
            throw new NotImplementedException();
        }

        public object GetDecoratedNavigationPropertyByMockEntity(ODataSelectManyQueryOperation operation)
        {
            throw new NotImplementedException();
        }

        public long CountRemoteCountedNavigationPropertyByMockEntity(ODataSelectManyQueryOperation operation)
        {
            return 10;            
        }

        public object CountRemoteCountedProperties(ODataSelectManyQueryOperation operation)
        {
            throw new NotImplementedException();
        }

        public object MockEntityWithCollectionCountAttributeCount(ODataQueryOperation operation)
        {
            throw new NotImplementedException();
        }
    }
}