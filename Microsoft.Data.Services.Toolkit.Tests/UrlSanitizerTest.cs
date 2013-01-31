namespace Microsoft.Data.Services.Toolkit.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    [TestClass]
    public class UrlSanitizerTest
    {
        [TestMethod]
        public void ShouldEncodeTheUrl()
        {
            var urlSettings = UrlSettingsAttribute.GetServiceUrlSettings(GetType());
            var urlSanitizer = new UrlSanitizer(urlSettings);

            string url = "http://odata.netflix.com/Catalog/Movies?$filter=substringof('Thelma &', ShortName)";
            
            var sanitizedUri = urlSanitizer.Sanitize(url);

            Assert.AreEqual("http://odata.netflix.com/Catalog/Movies?$filter=substringof('Thelma%20%26',%20ShortName)", sanitizedUri.AbsoluteUri);
        }
    }
}
