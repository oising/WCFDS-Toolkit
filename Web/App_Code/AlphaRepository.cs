using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Data.Services.Toolkit.QueryModel;
using Models;

/// <summary>
/// Summary description for SimpleEntityRepository
/// </summary>
public class AlphaRepository
{
    public AlphaRepository()
    {
    }

    // NOTE: parameter name(s) should match primary key property name(s) - case is unimportant.
    public MyAlphaEntity GetOne(string myId)
    {
        Guid key = Guid.Parse(myId);
        return new MyAlphaEntity()
                   {
                       MyId = key,
                       RelatedBeta =
                           new MyBetaEntity
                               {
                                   MyId = Guid.NewGuid(),
                                   ComplexTypeCollectionValueProperty =
                                       new[]
                                           {
                                               new MyComplexType() {Size = 1, Name = "foo1"},
                                               new MyComplexType() {Size = 2, Name = "foo2"},
                                               new MyComplexType() {Size = 3, Name = "foo3"},
                                           }
                               },
                       ComplexTypeCollectionValueProperty = Enumerable.Empty<MyComplexType>()
                   };
    }

    public IEnumerable<MyAlphaEntity> GetAll(ODataQueryOperation operation)
    {
        for (int i = 0; i < 5; i++)
        {
            yield return GetOne(Guid.NewGuid().ToString());
        }
    } 
}