using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using Microsoft.Data.Services.Toolkit.QueryModel;
using Models;

/// <summary>
/// Summary description for BetaRepository
/// </summary>
public class BetaRepository
{
    public BetaRepository()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public MyBetaEntity GetOne(string myId)
    {
        Guid key = Guid.Parse(myId);
        return new MyBetaEntity()
                   {
                       MyId = key,
                       ComplexTypeCollectionValueProperty =
                           new[]
                               {
                                   new MyComplexType() {Size = 1, Name = "foo1"},
                                   new MyComplexType() {Size = 2, Name = "foo2"},
                                   new MyComplexType() {Size = 3, Name = "foo3"},
                               }
                   };
    }

    public IEnumerable<MyBetaEntity> GetAll(ODataQueryOperation operation)
    {
        for (int i = 0; i < 5; i++)
        {
            yield return GetOne(Guid.NewGuid().ToString());
        }
    }

    // Example: /MyService.svc/Alpha(guid'8bbda742-ea9d-4e33-ba10-39a5ac090195')/$links/RelatedBeta
    public MyBetaEntity GetRelatedBetaByMyAlphaEntity(string myId)
    {
        return GetOne(Guid.NewGuid().ToString());
    }
}