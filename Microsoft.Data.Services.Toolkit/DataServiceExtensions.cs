namespace Microsoft.Data.Services.Toolkit
{
    using System.Data.Services;
    using System.Linq;

    /// <summary>
    /// Extension methods for the DataService class.
    /// </summary>
    public static class DataServiceExtensions
    {
        public static DataServiceConfiguration DataServiceConfiguration<T>(this DataService<T> dataService)
        {
            return (DataServiceConfiguration)typeof(DataService<>).GetInterfaces()
                                                                  .SingleOrDefault(i => i.Name == "IDataService")
                                                                  .GetProperty("Configuration")
                                                                  .GetValue(dataService, null);
        }
    }
}
