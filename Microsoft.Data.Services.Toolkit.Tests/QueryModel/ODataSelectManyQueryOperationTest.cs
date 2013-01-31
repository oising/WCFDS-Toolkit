namespace Microsoft.Data.Services.Toolkit.Tests.QueryModel
{
    using System.Collections.Generic;
    using Toolkit.QueryModel;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ODataSelectManyQueryOperationTest
    {
        [TestMethod]
        public void ItShouldReturnNavigationPropertyAsPropertyInfoOutOfType()
        {
            var operation = new ODataSelectManyQueryOperation
                                {
                                    OfType = typeof(MockEntity),
                                    Keys = new Dictionary<string, string> { { "key", "key" } },
                                    NavigationProperty = "NavigationProperty"
                                };

            var expectedProperty = typeof(MockEntity).GetProperty("NavigationProperty");
            Assert.AreEqual(operation.NavigationPropertyType, typeof(MockNavigationProperty));
            Assert.AreEqual(expectedProperty, operation.NavigationPropertyInfo);
        }
    }
}
