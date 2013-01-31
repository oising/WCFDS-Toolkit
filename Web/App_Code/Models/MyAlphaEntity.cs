using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using Microsoft.Data.Services.Toolkit.QueryModel;

namespace Models
{
    /// <summary>
    /// Summary description for MyEntity
    /// </summary>
    [DataServiceKey("MyId")]
    [DataServiceEntity]
    [ETag("ModifiedDateUTC")]
    public class MyAlphaEntity
    {
        public MyAlphaEntity()
        {
            MyId = Guid.NewGuid();
        }

        public Guid MyId { get; set; }

        public string Description { get; set; }

        public DateTimeOffset ModifiedDateUTC { get; set; }

        public MyComplexType ComplexTypeProperty { get; set; }

        public IEnumerable<MyComplexType> ComplexTypeCollectionValueProperty { get; set; }

        [ForeignProperty]
        public MyBetaEntity RelatedBeta { get; set; }
    }
}