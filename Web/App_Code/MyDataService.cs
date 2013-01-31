using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel;
using System.Web;
using Microsoft.Data.Services.Toolkit;
using Microsoft.Data.Services.Toolkit.QueryModel;
using Models;

/// <summary>
/// Summary description for MyDataContext
/// </summary>
[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
public class MyDataService : ODataService<MyDataContext>
{
    public MyDataService()
    {
    }

    public static void InitializeService(DataServiceConfiguration config)
    {
        // TODO: set rules to indicate which entity sets and service operations are visible, updatable, etc.
        // Examples:
        // config.SetEntitySetAccessRule("MyEntityset", EntitySetRights.AllRead);
        // config.SetServiceOperationAccessRule("MyServiceOperation", ServiceOperationRights.All);
        
        config.DataServiceBehavior.IncludeAssociationLinksInResponse = true;
        config.SetEntitySetAccessRule("*", EntitySetRights.All);
        config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3; //.V2;
        config.UseVerboseErrors = true;
    }
}