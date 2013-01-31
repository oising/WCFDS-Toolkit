namespace Microsoft.Data.Services.Toolkit.Tests.QueryModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using Moq;
    using Stubs;
    using Toolkit.QueryModel;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ODataQueryProviderTest
    {
        [TestMethod]
        public void ItShouldInvokeGetOneForODataGetOneQueryOperation()
        {
            var repository = new Mock<MockeableRepository>();
            var operation = new ODataSelectOneQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "key", "foo" } } };
            repository.Setup(r => r.GetOne(operation)).Returns(() => new MockEntity()).Verifiable();

            var provider = new ODataQueryProvider<MockEntity>(n => repository.Object);
            var result = provider.ExecuteQuery(operation);

            repository.VerifyAll();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ItShouldInvokeGetOneForODataGetOneQueryOperationWithProjection()
        {
            var repository = new Mock<MockeableRepository>();

            Expression<Func<MockEntity, dynamic>> projectionExpression = e => e == null ? null : new { e.Value };

            var operation = new ODataSelectOneQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "foo", "foo" } }, ProjectionExpression = projectionExpression, ProjectedType = typeof(object) };
            repository.Setup(r => r.GetOne(operation)).Returns(() => new MockEntity { Value = "value" }).Verifiable();

            var provider = new ODataQueryProvider<MockEntity>(n => repository.Object);
            var result = (IEnumerable<object>)provider.ExecuteQuery(operation);

            repository.VerifyAll();
            Assert.AreEqual(result.Cast<dynamic>().First().Value, "value");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(DataServiceException))]
        public void ItShouldUnwrapTargetOfInvocationExceptions()
        {
            Expression<Func<MockEntity, dynamic>> projectionExpression = e => e == null ? null : new { e.Value };
            var operation = new ODataSelectOneQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "key", "not_found" } }, ProjectionExpression = projectionExpression, ProjectedType = typeof(object) };

            var repository = new Mock<MockeableRepository>();
            repository.Setup(r => r.GetOne(operation)).Throws(new WebException());

            var provider = new ODataQueryProvider<MockEntity>(n => repository.Object);
            provider.ExecuteQuery(operation);

            Assert.Inconclusive();
        }

        [TestMethod]
        public void ItShouldExecuteProjectionEvenWhenItHandlesAll()
        {
            Expression<Func<MockEntity, dynamic>> projectionExpression = e => e == null ? null : new { e.Value };

            var operation = new ODataSelectOneQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "key", "key" } }, ProjectionExpression = projectionExpression, ProjectedType = typeof(object) };
            var provider = new ODataQueryProvider<MockEntity>(n => new HandlesAllRepository(new MockEntity { Value = "value" }));
            var result = (IEnumerable<object>)provider.ExecuteQuery(operation);

            dynamic item = result.FirstOrDefault();
            Assert.AreEqual(item.Value, "value");
            Assert.IsNotInstanceOfType(result.FirstOrDefault(), typeof(MockEntity));
        }

        [TestMethod]
        public void ItShouldExecuteProjectionEvenWhenItHandlesSelect()
        {
            Expression<Func<MockEntity, dynamic>> projectionExpression = e => e == null ? null : new { e.Value };

            var operation = new ODataSelectOneQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "key", "key" } }, ProjectionExpression = projectionExpression, ProjectedType = typeof(object) };
            var provider = new ODataQueryProvider<MockEntity>(n => new HandlesSelectRepository(new MockEntity { Value = "value" }));
            var result = (IEnumerable<object>)provider.ExecuteQuery(operation);

            dynamic item = result.FirstOrDefault();
            Assert.AreEqual(item.Value, "value");
            Assert.IsNotInstanceOfType(result.FirstOrDefault(), typeof(MockEntity));
        }

        [TestMethod]
        public void ItShouldRemoteDecoratedCountNavigationProperties()
        {
            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "key", "key" } }, NavigationProperty = "RemoteCountedNavigationProperty", IsCountRequest = true };
            var provider = new ODataQueryProvider<MockEntity>(n => new RepositoryStub());
            var result = (long)provider.ExecuteQuery(operation);

            Assert.IsNotNull(result);
            Assert.AreEqual(result, 10);
        }

        [TestMethod]
        public void ItShouldHandleSelectManyOperationsWhenNavigationPropertyIsNotDecorated()
        {
            var navProp = new MockNavigationProperty { Value = "provided" };
            var entity = new MockEntity { NavigationProperty = navProp };

            var repository = new Mock<MockeableRepository>();
            repository.Setup(r => r.GetOne(It.Is<ODataSelectOneQueryOperation>(o => o.Key == "key" && o.OfType == typeof(MockEntity))))
                      .Returns(entity);

            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "key", "key" } }, NavigationProperty = "NavigationProperty" };

            var provider = new ODataQueryProvider<MockEntity>(n => repository.Object);
            var result = (IEnumerable<MockNavigationProperty>)provider.ExecuteQuery(operation);

            repository.VerifyAll();
            Assert.AreEqual(result.First(), navProp);
        }

        [TestMethod]
        public void ItShouldCountSelectManyOperationsWhenNavigationPropertyIsNotDecorated()
        {
            var navProp = new MockNavigationProperty { Value = "provided" };
            var entity = new MockEntity { NavigationProperties = new[] { navProp } };

            var repository = new Mock<MockeableRepository>();
            repository.Setup(r => r.GetOne(It.Is<ODataSelectOneQueryOperation>(o => o.Key == "key" && o.OfType == typeof(MockEntity))))
                      .Returns(entity);

            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "key", "key" } }, NavigationProperty = "NavigationProperties", IsCountRequest = true };

            var provider = new ODataQueryProvider<MockEntity>(n => repository.Object);
            var result = (long)provider.ExecuteQuery(operation);

            repository.VerifyAll();
            Assert.AreEqual(result, 1);
        }

        [TestMethod]
        public void ItShouldExecuteProjectionsOnSelectManyQueryOperation()
        {
            var navProp = new MockNavigationProperty { Value = "provided" };
            var entity = new MockEntity { NavigationProperties = new[] { navProp } };

            var repository = new Mock<MockeableRepository>();
            repository.Setup(r => r.GetOne(It.Is<ODataSelectOneQueryOperation>(o => o.Key == "key" && o.OfType == typeof(MockEntity))))
                      .Returns(entity);

            Expression<Func<MockNavigationProperty, dynamic>> projectionExpression = e => e == null ? null : new { e.Value };

            var operation = new ODataSelectManyQueryOperation
                                {
                                    OfType = typeof(MockEntity),
                                    Keys = new Dictionary<string, string> { { "key", "key" } },
                                    NavigationProperty = "NavigationProperties",
                                    ProjectionExpression = projectionExpression,
                                    ProjectedType = typeof(object)
                                };

            var provider = new ODataQueryProvider<MockEntity>(n => repository.Object);
            var result = (IEnumerable<dynamic>)provider.ExecuteQuery(operation);

            repository.VerifyAll();
            Assert.AreEqual(result.First().Value, "provided");
        }

        [TestMethod]
        public void ItShouldPerformLinq2ObjectsFilteringWhenMethodDoesntProvidedOutOfTheBox()
        {
            var entity = new MockEntity
            {
                NavigationProperties = new[]
                {
                    new MockNavigationProperty { Value = "provided_one" }, 
                    new MockNavigationProperty { Value = "provided_two" }
                }
            };

            var repository = new Mock<MockeableRepository>();
            repository.Setup(r => r.GetOne(It.Is<ODataSelectOneQueryOperation>(o => o.Key == "key" && o.OfType == typeof(MockEntity))))
                      .Returns(entity);

            Expression<Func<MockNavigationProperty, bool>> filter = e => e.Value == "provided_one";

            var operation = new ODataSelectManyQueryOperation
                                {
                                    OfType = typeof(MockEntity),
                                    Keys = new Dictionary<string, string> { { "key", "key" } },
                                    NavigationProperty = "NavigationProperties",
                                    FilterExpression = filter
                                };

            var provider = new ODataQueryProvider<MockEntity>(n => repository.Object);
            var result = (IEnumerable<dynamic>)provider.ExecuteQuery(operation);

            repository.VerifyAll();
            Assert.AreEqual(result.First().Value, "provided_one");
            Assert.AreEqual(result.Count(), 1);
        }

        [TestMethod]
        public void ItShouldSortUsingLinq2ObjectsImplementationWhenTheRepositoryDoesntProvideIt()
        {
            var entity = new MockEntity
            {
                NavigationProperties = new[]
                {
                    new MockNavigationProperty { Value = "2. provided" }, 
                    new MockNavigationProperty { Value = "1. provided" }
                }
            };

            var repository = new Mock<MockeableRepository>();
            repository.Setup(r => r.GetOne(It.Is<ODataSelectOneQueryOperation>(o => o.Key == "key" && o.OfType == typeof(MockEntity))))
                      .Returns(entity);

            Expression<Func<MockNavigationProperty, string>> order = e => e.Value;

            var operation = new ODataSelectManyQueryOperation
                                {
                                    OfType = typeof(MockEntity),
                                    Keys = new Dictionary<string, string> { { "key", "key" } },
                                    NavigationProperty = "NavigationProperties",
                                    OrderStack = new Stack<ODataOrderExpression>(new[] { new ODataOrderExpression { OrderMethodName = "OrderBy", Expression = order } })
                                };

            var provider = new ODataQueryProvider<MockEntity>(n => repository.Object);
            var result = (IEnumerable<dynamic>)provider.ExecuteQuery(operation);

            repository.VerifyAll();
            Assert.AreEqual(result.First().Value, "1. provided");
        }

        [TestMethod]
        public void ItShouldExecuteSkipWithLinq2ObjectWhenTheUnderlyingMethodDoesntProvideIt()
        {
            var entity = new MockEntity
            {
                NavigationProperties = new[]
                {
                    new MockNavigationProperty { Value = "2. provided" }, 
                    new MockNavigationProperty { Value = "1. provided" }
                }
            };

            var repository = new Mock<MockeableRepository>();
            repository.Setup(r => r.GetOne(It.Is<ODataSelectOneQueryOperation>(o => o.Key == "key" && o.OfType == typeof(MockEntity))))
                      .Returns(entity);

            var operation = new ODataSelectManyQueryOperation
                                {
                                    OfType = typeof(MockEntity),
                                    Keys = new Dictionary<string, string> { { "key", "key" } },
                                    NavigationProperty = "NavigationProperties",
                                    SkipCount = 1
                                };

            var provider = new ODataQueryProvider<MockEntity>(n => repository.Object);
            var result = (IEnumerable<dynamic>)provider.ExecuteQuery(operation);

            repository.VerifyAll();
            Assert.AreEqual(result.First().Value, "1. provided");
            Assert.AreEqual(result.Count(), 1);
        }

        [TestMethod]
        public void ItShouldExecuteTopWithLinq2ObjectWhenTheUnderlyingMethodDoesntProvideIt()
        {
            var entity = new MockEntity
            {
                NavigationProperties = new[]
                {
                    new MockNavigationProperty { Value = "2. provided" }, 
                    new MockNavigationProperty { Value = "1. provided" }
                }
            };

            var repository = new Mock<MockeableRepository>();
            repository.Setup(r => r.GetOne(It.Is<ODataSelectOneQueryOperation>(o => o.Key == "key" && o.OfType == typeof(MockEntity))))
                      .Returns(entity);

            var operation = new ODataSelectManyQueryOperation
                                {
                                    OfType = typeof(MockEntity),
                                    Keys = new Dictionary<string, string> { { "key", "key" } },
                                    NavigationProperty = "NavigationProperties",
                                    TopCount = 1
                                };

            var provider = new ODataQueryProvider<MockEntity>(n => repository.Object);
            var result = (IEnumerable<dynamic>)provider.ExecuteQuery(operation);

            repository.VerifyAll();
            Assert.AreEqual(result.First().Value, "2. provided");
            Assert.AreEqual(result.Count(), 1);
        }

        [TestMethod]
        public void ItShouldRemoteCountMockEntity2WhenCollectionCountSupportsIt()
        {
            var operation = new ODataQueryOperation { OfType = typeof(MockEntity2), IsCountRequest = true };
            var provider = new ODataQueryProvider<MockEntity>(n => new MockRepository2());
            var result = (long)provider.ExecuteQuery(operation);

            Assert.AreEqual(result, 1);
        }

        //[TestMethod]
        //public void ItShouldNotThrowWhenRepositoryReturnsNullOnGetAll()
        //{
        //    var repository = new Mock<IRepository<MockEntity>>();
        //    repository.Setup(r => r.GetAll()).Returns(default(MockEntity[]));
        //    var provider = new ODataQueryProvider<MockEntity>(n => repository.Object);
        //    var operation = new ODataQueryOperation { OfType = typeof(MockEntity) };
        //    var result = provider.ExecuteQuery(operation);

        //    Assert.IsNull(result);
        //}

        [RepositoryBehavior(HandlesEverything = true)]
        private class HandlesAllRepository
        {
            private readonly MockEntity result;

            public HandlesAllRepository()
                : this(null)
            {
            }

            public HandlesAllRepository(MockEntity result)
            {
                this.result = result;
            }

            public object GetAll(ODataQueryOperation operation)
            {
                throw new NotImplementedException();
            }

            public object GetOne(ODataSelectOneQueryOperation operation)
            {
                return this.result ?? new MockEntity();
            }
        }

        [RepositoryBehavior(HandlesSelect = true)]
        private class HandlesSelectRepository
        {
            private readonly MockEntity result;

            public HandlesSelectRepository()
                : this(null)
            {
            }

            public HandlesSelectRepository(MockEntity result)
            {
                this.result = result;
            }

            public object GetAll(ODataQueryOperation operation)
            {
                throw new NotImplementedException();
            }

            public object GetOne(ODataSelectOneQueryOperation operation)
            {
                return this.result ?? new MockEntity();
            }
        }

        private class MockRepository2
        {
            public object GetAll(ODataQueryOperation operation)
            {
                throw new NotImplementedException();
            }

            public long MockEntity2Count(ODataQueryOperation query)
            {
                return 1;
            }

            public object GetOne(ODataSelectOneQueryOperation operation)
            {
                throw new NotImplementedException();
            }
        }

        [CollectionCount]
        private class MockEntity2
        {
        }
    }
}
