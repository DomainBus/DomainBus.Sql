using System;
using DomainBus.Configuration;
using DomainBus.Dispatcher.Server;
using DomainBus.Sql.Server;
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

        public static void WithSqlStorage(this DispatchServerConfiguration cfg,IDbFactory connection,string dbSchema="")
        {
            ServerStateStorage.Init(connection,dbSchema);
            cfg.Storage=new ServerStateStorage(connection);
        }
    }
}