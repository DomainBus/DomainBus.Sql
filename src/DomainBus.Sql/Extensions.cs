using System;
using DomainBus.Configuration;
using SqlFu;

namespace DomainBus.Sql
{
    public static class Extensions
    {
       
        public static IConfigureHost WithSqlStorages(this IConfigureHost host,IDbFactory connection ,Action<StoragesConfiguration> cfgAction)
        {
            cfgAction.MustNotBeNull();
            var cfg=new StoragesConfiguration(host,connection);
            cfgAction(cfg);
            return host;
        }
    }
}