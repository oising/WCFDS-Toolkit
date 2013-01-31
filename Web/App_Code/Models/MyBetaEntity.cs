using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;

namespace Models
{
    /// <summary>
    /// Summary description for MyEntity
    /// </summary>
    [DataServiceKey("MyId")]
    [DataServiceEntity]
    [ETag("ModifiedDateUTC")]
    public class MyBetaEntity
    {
        public MyBetaEntity()
        {
            MyId = Guid.NewGuid();
        }

        public Guid MyId { get; set; }

        public string Description { get; set; }

        public DateTimeOffset ModifiedDateUTC { get; set; }

        public MyComplexType ComplexTypeProperty { get; set; }

        // can also use MyComplexType[]
        public IEnumerable<MyComplexType> ComplexTypeCollectionValueProperty { get; set; } 
    }
}