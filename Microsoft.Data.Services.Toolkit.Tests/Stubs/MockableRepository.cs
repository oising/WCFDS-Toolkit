namespace Microsoft.Data.Services.Toolkit.Tests.Stubs
{
    using Microsoft.Data.Services.Toolkit.QueryModel;

    public abstract class MockeableRepository
    {
        public abstract object GetAll(ODataQueryOperation operation);

        public abstract object GetOne(ODataSelectOneQueryOperation operation);
    }
}
