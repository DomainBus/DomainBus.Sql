using System;
using DomainBus.Configuration;
using DomainBus.Dispatcher.Server;
using DomainBus.Sql.Communicators;
using DomainBus.Sql.Server;
using SqlFu;
using SqlFu.Builders;

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
        public static void ReceiveFromClientsBySql(this DispatchServerConfiguration cfg,IDbFactory connection,string table=CommunicatorTable,string dbSchema=CommunicatorSchema)
        {
            new ClientToServerRowCreator(connection).WithTableName(table, dbSchema).IfExists(TableExistsAction.Ignore).Create();
            var configReceiver = new ConfigReceiver(connection);
           
            cfg.EndpointUpdatesNotifier=configReceiver;

            var fromClient = new ReceiverFromClient(connection);
           
            cfg.MessageNotifier = fromClient;
        }
         
        public const string CommunicatorTable = "dbus_client_to_server";
        public const string CommunicatorSchema = "";

        public static IConfigureDispatcher CommunicateBySqlStorage(this IConfigureDispatcher cfg,IDbFactory connection,string table=CommunicatorTable,string dbSchema=CommunicatorSchema,TableExistsAction ifExists=TableExistsAction.Ignore)
        {
            new ClientToServerRowCreator(connection).WithTableName(table,dbSchema).IfExists(ifExists).Create();
            return cfg.TalkUsing(new ClientToServer(connection));
        }
    }
}