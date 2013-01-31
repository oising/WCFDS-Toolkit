namespace Microsoft.Data.Services.Toolkit.Tests.QueryModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Stubs;
    using Toolkit.QueryModel;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ODataOperationResolverTest
    {
        [TestMethod]
        public void ShouldResolveRepositoryForOneTypeBasedOperation()
        {
            string resolvedTypeName = null;
            var operation = new ODataSelectOneQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "key", "foo" } } };
            Func<string, object> resolver = a => { resolvedTypeName = a; return new RepositoryStub(); };

            var repository = ODataOperationResolver.For(operation).Repository(resolver);

            Assert.AreEqual(resolvedTypeName, typeof(MockEntity).FullName);
            Assert.IsInstanceOfType(repository, typeof(RepositoryStub));
        }

        [TestMethod]
        public void ShouldResolveRepositoryForRootCollectionOneTypeBasedOperation()
        {
            string resolvedTypeName = null;
            var operation = new ODataQueryOperation { OfType = typeof(MockEntity) };
            Func<string, object> resolver = a => { resolvedTypeName = a; return new RepositoryStub(); };

            var repository = ODataOperationResolver.For(operation).Repository(resolver);

            Assert.AreEqual(resolvedTypeName, typeof(MockEntity).FullName);
            Assert.IsInstanceOfType(repository, typeof(RepositoryStub));
        }

        [TestMethod]
        public void ShouldResolveRepositoryForSelectManyTypeBasedOperationWhenNavPropIsNotDecorated()
        {
            string resolvedTypeName = null;
            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "key", "foo" } }, NavigationProperty = "NavigationProperty" };
            Func<string, object> resolver = a => { resolvedTypeName = a; return new RepositoryStub(); };

            var repository = ODataOperationResolver.For(operation).Repository(resolver);

            Assert.AreEqual(resolvedTypeName, typeof(MockEntity).FullName);
            Assert.IsInstanceOfType(repository, typeof(RepositoryStub));
        }

        [TestMethod]
        public void ShouldResolveRepositoryForSelectManyTypeBasedOperationWhenNavPropIsDecorated()
        {
            string resolvedTypeName = null;
            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "key", "foo" } }, NavigationProperty = "DecoratedNavigationProperty" };
            Func<string, object> resolver = a => { resolvedTypeName = a; return new RepositoryStub(); };

            var repository = ODataOperationResolver.For(operation).Repository(resolver);

            Assert.AreEqual(resolvedTypeName, typeof(MockNavigationProperty).FullName);
            Assert.IsInstanceOfType(repository, typeof(RepositoryStub));
        }

        [TestMethod]
        public void ShouldResolveRepositoryForSelectManyCountTypeBasedOperationWhenNavPropIsNotDecorated()
        {
            string resolvedTypeName = null;
            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "key", "foo" } }, NavigationProperty = "NavigationProperty", IsCountRequest = true };
            Func<string, object> resolver = a => { resolvedTypeName = a; return new RepositoryStub(); };

            var repository = ODataOperationResolver.For(operation).Repository(resolver);

            Assert.AreEqual(resolvedTypeName, typeof(MockEntity).FullName);
            Assert.IsInstanceOfType(repository, typeof(RepositoryStub));
        }

        [TestMethod]
        public void ShouldResolveRepositoryForSelectManyCountTypeBasedOperationWhenNavPropIsDecorated()
        {
            string resolvedTypeName = null;
            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "key", "foo" } }, NavigationProperty = "RemoteCountedNavigationProperty", IsCountRequest = true };
            Func<string, object> resolver = a => { resolvedTypeName = a; return new RepositoryStub(); };

            var repository = ODataOperationResolver.For(operation).Repository(resolver);

            Assert.AreEqual(resolvedTypeName, typeof(MockNavigationProperty).FullName);
            Assert.IsInstanceOfType(repository, typeof(RepositoryStub));
        }

        [TestMethod]
        public void ShouldGetMethodInfoForODataSelectOneQueryOperation()
        {
            var operation = new ODataSelectOneQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "key", "foo" } } };

            var method = ODataOperationResolver.For(operation).Method(new RepositoryStub());

            Assert.AreEqual("RepositoryStub", method.DeclaringType.Name);
            Assert.AreEqual("GetOne", method.Name);
        }

        [TestMethod]
        public void ShouldGetMethodInfoForODataSelectManyQueryOperationIfThereIsAttributeWithNameSpecified()
        {
            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "key", "foo" } }, NavigationProperty = "DecoratedNavigationPropertyWithMethodName" };

            var method = ODataOperationResolver.For(operation).Method(new RepositoryStub());

            Assert.AreEqual("RepositoryStub", method.DeclaringType.Name);
            Assert.AreEqual("MockMethodForDecoratedNavigationProperty", method.Name);
        }

        [TestMethod]
        public void ShouldGetMethodInfoForODataSelectManyQueryOperationIfThereIsAttributeWithoutNameSpecified()
        {
            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "key", "foo" } }, NavigationProperty = "DecoratedNavigationProperty" };

            var method = ODataOperationResolver.For(operation).Method(new RepositoryStub());

            Assert.AreEqual("RepositoryStub", method.DeclaringType.Name);
            Assert.AreEqual("GetDecoratedNavigationPropertyByMockEntity", method.Name);
        }

        [TestMethod]
        public void ShouldGetMethodInfoForODataSelectManyQueryOperationIfThereIsNoAttribute()
        {
            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "key", "foo" } }, NavigationProperty = "NavigationProperty" };

            var method = ODataOperationResolver.For(operation).Method(new RepositoryStub());

            Assert.AreEqual("RepositoryStub", method.DeclaringType.Name);
            Assert.AreEqual("GetOne", method.Name);
        }

        [TestMethod]
        public void ShouldGetMethodInfoForODataCountQueryOperationIfThereIsAttributeWithoutNameSpecified()
        {
            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "key", "foo" } }, NavigationProperty = "RemoteCountedNavigationProperty", IsCountRequest = true };

            var method = ODataOperationResolver.For(operation).Method(new RepositoryStub());

            Assert.AreEqual("RepositoryStub", method.DeclaringType.Name);
            Assert.AreEqual("CountRemoteCountedNavigationPropertyByMockEntity", method.Name);
        }

        [TestMethod]
        public void ShouldGetMethodInfoForODataCountQueryOperationIfThereIsAttributeWithNameSpecified()
        {
            var operation = new ODataSelectManyQueryOperation { OfType = typeof(MockEntity), Keys = new Dictionary<string, string> { { "key", "foo" } }, NavigationProperty = "RemoteCountedNavigationPropertyWithMethodName", IsCountRequest = true };

            var method = ODataOperationResolver.For(operation).Method(new RepositoryStub());

            Assert.AreEqual("RepositoryStub", method.DeclaringType.Name);
            Assert.AreEqual("CountRemoteCountedProperties", method.Name);
        }

        [TestMethod]
        public void ShouldGetMethodInfoForODataQueryOperation()
        {
            var operation = new ODataQueryOperation { OfType = typeof(MockEntity) };

            var method = ODataOperationResolver.For(operation).Method(new RepositoryStub());

            Assert.AreEqual("RepositoryStub", method.DeclaringType.Name);
            Assert.AreEqual("GetAll", method.Name);
        }

        [TestMethod]
        public void ShouldGetMethodInfoForODataRootCountQueryOperationWhenNoCollectionCountAttribute()
        {
            var operation = new ODataQueryOperation { OfType = typeof(MockEntity), IsCountRequest = true };

            var method = ODataOperationResolver.For(operation).Method(new RepositoryStub());

            Assert.AreEqual("RepositoryStub", method.DeclaringType.Name);
            Assert.AreEqual("GetAll", method.Name);
        }

        [TestMethod]
        public void ShouldGetMethodInfoForODataRootCountQueryOperationWhenCollectionCountAttribute()
        {
            var operation = new ODataQueryOperation { OfType = typeof(MockEntityWithCollectionCountAttribute), IsCountRequest = true };

            var method = ODataOperationResolver.For(operation).Method(new RepositoryStub());

            Assert.AreEqual("RepositoryStub", method.DeclaringType.Name);
            Assert.AreEqual("MockEntityWithCollectionCountAttributeCount", method.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(MissingMethodException))]
        public void ShouldThrowAProperExceptionWhenNoSuitableMethodFound()
        {
            var operation = new ODataQueryOperation { OfType = typeof(MockEntityWithCollectionCountAttribute) };

            ODataOperationResolver.For(operation).Method(new IncompleteEntityRepository());
        }

        [TestMethod]
        public void ShouldThrowAProperExceptionWhenNoSuitableMethodFoundWithANiceMessage()
        {
            var operation = new ODataQueryOperation { OfType = typeof(MockEntityWithCollectionCountAttribute) };

            try
            {
                ODataOperationResolver.For(operation).Method(new IncompleteEntityRepository());
            }
            catch (Exception e)
            {
                Assert.AreEqual("The method GetAll cannot be found on the repository IncompleteEntityRepository.", e.Message);
            }
        }
        
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ShouldThrowAProperExceptionWhenTheRepositoryIsNotFound()
        {
            var operation = new ODataQueryOperation { OfType = typeof(MockEntityWithCollectionCountAttribute) };

            ODataOperationResolver.For(operation).Repository(s => null);
        }

        [TestMethod]
        public void ShouldThrowAProperExceptionWhenNoRepositoryFoundWithANiceMessage()
        {
            var operation = new ODataQueryOperation { OfType = typeof(MockEntityWithCollectionCountAttribute) };

            try
            {
                ODataOperationResolver.For(operation).Repository(s => null);
            }
            catch (Exception e)
            {
                Assert.AreEqual("No repository found for MockEntityWithCollectionCountAttribute.", e.Message);
            }
        }

        [TestMethod]
        public void ShouldGetArgumentsForGetOneOpertionWhenGivenMethodHasGetOneQueryOperation()
        {
            var operation = new ODataSelectOneQueryOperation { OfType = typeof(MockEntityWithCollectionCountAttribute), Keys = new Dictionary<string, string> { { "key", "key" } } };
            var method = ODataOperationResolver.For(operation).Method(new RepositoryStub());
            var arguments = ODataOperationResolver.For(operation).Arguments(method);

            Assert.AreEqual(arguments.First(), operation);
        }

        [TestMethod]
        public void ShouldGetArgumentsForGetOneOpertionWhenGivenMethodHasStringAsOpertion()
        {
            var operation = new ODataSelectOneQueryOperation { OfType = typeof(MockEntityWithCollectionCountAttribute), Keys = new Dictionary<string, string> { { "key", "key" } } };
            var method = ODataOperationResolver.For(operation).Method(new CustomRepositoryStub());
            var arguments = ODataOperationResolver.For(operation).Arguments(method);

            Assert.AreEqual(arguments.First(), "key");
        }

        [TestMethod]
        public void ShouldGetArgumentsForGetAllOpertionWhenGivenMethodHasQueryOperation()
        {
            var operation = new ODataQueryOperation { OfType = typeof(MockEntityWithCollectionCountAttribute) };
            var method = ODataOperationResolver.For(operation).Method(new RepositoryStub());
            var arguments = ODataOperationResolver.For(operation).Arguments(method);

            Assert.AreEqual(arguments.First(), operation);
        }

        [TestMethod]
        public void ShouldGetArgumentsForGetOneOpertionWhenGivenMethodHasNoArguments()
        {
            var operation = new ODataQueryOperation { OfType = typeof(MockEntityWithCollectionCountAttribute) };
            var method = ODataOperationResolver.For(operation).Method(new CustomRepositoryStub());
            var arguments = ODataOperationResolver.For(operation).Arguments(method);

            Assert.IsNull(arguments);
        }

        [TestMethod]
        public void ShouldGetOrderedArgumentsForExecution()
        {
            var operation = new ODataSelectOneQueryOperation
                                {
                                    OfType = typeof(Entity),
                                    Keys = new Dictionary<string, string> { { "parameterTwo", "valueTwo" }, { "parameterOne", "valueOne" } }
                                };

            var method = ODataOperationResolver.For(operation).Method(new EntityRepository());
            var arguments = ODataOperationResolver.For(operation).Arguments(method);

            Assert.AreEqual(arguments.First().ToString(), "valueOne");
            Assert.AreEqual(arguments.Last().ToString(), "valueTwo");
        }

        [TestMethod]
        public void ShouldGetOrderedArgumentsForExecutionWhenOneOfTheKeysIsNotRequiredOnTheMethod()
        {
            var operation = new ODataSelectOneQueryOperation
            {
                OfType = typeof(Entity),
                Keys = new Dictionary<string, string> { { "parameterTwo", "valueTwo" }, { "parameterOne", "valueOne" } }
            };

            var method = ODataOperationResolver.For(operation).Method(new EntityRepository2());
            var arguments = ODataOperationResolver.For(operation).Arguments(method);

            Assert.AreEqual(arguments.First().ToString(), "valueTwo");
            Assert.AreEqual(arguments.Last().ToString(), "valueTwo");
        }

        [TestMethod]
        public void ShouldGetOrderedArgumentsForExecutionWhenOneOfTheKeysIsSkippedRequiredOnTheMethod()
        {
            var operation = new ODataSelectOneQueryOperation
            {
                OfType = typeof(Entity),
                Keys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                           {
                               { "parameterTwo", "valueTwo" }, { "parameterOne", "valueOne" }, { "PaRaMeTeRThRee", "valueThree" }
                           }
            };

            var method = ODataOperationResolver.For(operation).Method(new EntityRepository3());
            var arguments = ODataOperationResolver.For(operation).Arguments(method);

            Assert.AreEqual(arguments.First().ToString(), "valueTwo");
            Assert.AreEqual(arguments.Last().ToString(), "valueThree");
        }

        internal class Entity
        {
        }

        internal class EntityRepository
        {
            public object GetOne(string parameterOne, string parameterTwo)
            {
                return default(object);
            }
        }

        internal class IncompleteEntityRepository
        {
            public object GetOne(string parameterOne, string parameterTwo)
            {
                return default(object);
            }
        }

        internal class EntityRepository2
        {
            public object GetOne(string parameterTwo)
            {
                return default(object);
            }
        }

        internal class EntityRepository3
        {
            public object GetOne(string parameterTwo, string parameterThree)
            {
                return default(object);
            }
        }
    }
}