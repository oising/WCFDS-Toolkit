namespace Microsoft.Data.Services.Toolkit.Tests.QueryModel
{
    using System.Collections.Generic;
    using Microsoft.Data.Services.Toolkit.QueryModel;

    public class MockEntity
    {
        public string Value { get; set; }

        public MockNavigationProperty NavigationProperty { get; set; }

        public IEnumerable<MockNavigationProperty> NavigationProperties { get; set; }

        [ForeignProperty]
        public MockNavigationProperty DecoratedNavigationProperty { get; set; }

        [ForeignProperty(RepositoryMethod = "MockMethodForDecoratedNavigationProperty")]
        public MockNavigationProperty DecoratedNavigationPropertyWithMethodName { get; set; }

        [ForeignPropertyCount]
        public MockNavigationProperty RemoteCountedNavigationProperty { get; set; }

        [ForeignProperty]
        [ForeignPropertyCount(RepositoryMethod = "CountRemoteCountedProperties")]
        public MockNavigationProperty RemoteCountedNavigationPropertyWithMethodName { get; set; }

        public MockComplexType ComplexTypeProperty { get; set; }

        public IEnumerable<MockComplexType> ComplexTypeCollectionValueEnumerableProperty { get; set; }

        public MockComplexType[] ComplexTypeCollectionValueAsArrayProperty { get; set; }
    }
}