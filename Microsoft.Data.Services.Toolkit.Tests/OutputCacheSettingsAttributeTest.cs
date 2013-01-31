namespace Microsoft.Data.Services.Toolkit.Tests
{
    using System.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OutputCacheSettingsAttributeTest
    {
        [TestMethod]
        public void ShouldDefaultExpirationTo30SecondsAndDefaultHttpCacheabilityToServerAndPrivate()
        {
            var cachableType = new CachableType();

            var cacheAttribute = 
                EnableOutputCacheAttribute.GetServiceOutputCacheSettings(cachableType.GetType());

            Assert.AreEqual(30, cacheAttribute.ExpiresInSeconds);
            Assert.AreEqual(HttpCacheability.ServerAndPrivate, cacheAttribute.HttpCacheability);
        }

        [TestMethod]
        public void ShouldReturnNullWhenNoOutCacheAttributeIsPresent()
        {
            var nonCachableType = new NonCachableType();

            var cacheAttribute =
                EnableOutputCacheAttribute.GetServiceOutputCacheSettings(nonCachableType.GetType());

            Assert.IsNull(cacheAttribute);
        }

        [TestMethod]
        public void ShouldBeAbleToSetCustomExpiration()
        {
            var cachableTypeWithCustomExpiration = new CachableTypeWithCustomExpiration();

            var cacheAttribute =
                EnableOutputCacheAttribute.GetServiceOutputCacheSettings(cachableTypeWithCustomExpiration.GetType());

            Assert.AreEqual(20, cacheAttribute.ExpiresInSeconds);
        }

        [TestMethod]
        public void ShouldBeAbleToSetCustomCacheability()
        {
            var cachableTypeWithCustomCacheability = new CacheableTypeWithCustomHttpCacheability();

            var cacheAttribute =
                EnableOutputCacheAttribute.GetServiceOutputCacheSettings(cachableTypeWithCustomCacheability.GetType());

            Assert.AreEqual(HttpCacheability.Public, cacheAttribute.HttpCacheability);
        }

        [EnableOutputCache]
        private class CachableType
        {
        }

        [EnableOutputCache(ExpiresInSeconds = 20)]
        private class CachableTypeWithCustomExpiration
        {
        }

        [EnableOutputCache(HttpCacheability = HttpCacheability.Public)]
        private class CacheableTypeWithCustomHttpCacheability
        {
        }

        private class NonCachableType
        {
        }
    }
}
