namespace Microsoft.Data.Services.Toolkit.Tests.QueryModel
{
    using System;
    using Moq;
    using Stubs;
    using Toolkit.QueryModel;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RepositoryBehaviorExtensionsTests
    {
        [TestMethod]
        public void ItShouldGetRepositoryBehaviorDecoratedOnTheClass()
        {
            var repository = new MockRepositoryWithBehavior();
            var method = repository.GetType().GetMethod("GetAll");
            var behavior = repository.GetBehavior(method);

            Assert.IsTrue(behavior.HandlesEverything);
        }

        [TestMethod]
        public void ItShouldGetRepositoryBehaviorForMethodOverridingDecoratedOnTheClass()
        {
            var repository = new MockRepositoryWithBehavior();
            var method = repository.GetType().GetMethod("BehaviorOverwritten");
            var behavior = repository.GetBehavior(method);

            Assert.IsFalse(behavior.HandlesEverything);
        }

        [TestMethod]
        public void ItShouldBehaviorForNonDecoratedTypesOrMethod()
        {
            var repository = new Mock<MockeableRepository>().Object;
            var method = repository.GetType().GetMethod("GetAll");
            var behavior = repository.GetBehavior(method);

            Assert.IsNotNull(behavior);
            Assert.IsFalse(behavior.HandlesEverything);
        }

        [RepositoryBehavior(HandlesEverything = true)]
        internal class MockRepositoryWithBehavior
        {
            public object GetAll(ODataQueryOperation operation)
            {
                throw new NotImplementedException();
            }

            public object GetOne(ODataSelectOneQueryOperation operation)
            {
                throw new NotImplementedException();
            }

            [RepositoryBehavior(HandlesEverything = false)]
            public object BehaviorOverwritten()
            {
                throw new NotImplementedException();
            }
        }
    }
}
