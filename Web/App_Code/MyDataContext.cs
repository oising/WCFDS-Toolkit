using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Data.Services.Toolkit.QueryModel;
using Models;

/// <summary>
/// Summary description for MyDataContext
/// </summary>
public class MyDataContext : ODataContext
{
    public MyDataContext()
    {
    }

    public IQueryable<MyAlphaEntity> Alpha
    {
        get
        {
            return CreateQuery<MyAlphaEntity>();
        }
    }

    public IQueryable<MyBetaEntity> Beta
    {
        get
        {
            return CreateQuery<MyBetaEntity>();
        }
    } 

    public override object RepositoryFor(string fullTypeName)
    {
        switch (fullTypeName)
        {
            case "Models.MyAlphaEntity":
                return new AlphaRepository();

            case "Models.MyBetaEntity":
                return new BetaRepository();

            default:
                throw new ArgumentOutOfRangeException("Unknown entity: " + fullTypeName);
        }
    }
}