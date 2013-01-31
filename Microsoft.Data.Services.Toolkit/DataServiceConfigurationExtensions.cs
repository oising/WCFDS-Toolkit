namespace Microsoft.Data.Services.Toolkit
{
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Reflection;

    /// <summary>
    /// Extension methods to get information from the Data Service configuration.
    /// </summary>
    public static class DataServiceConfigurationExtensions
    {
        public static Dictionary<string, int> PageSizes(this DataServiceConfiguration configuration)
        {
            return (Dictionary<string, int>)configuration.GetType()
                                                         .InvokeMember("pageSizes", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, configuration, null);
        }
    }
}
