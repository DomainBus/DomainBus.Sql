using System;
using System.Data.SqlClient;
using DomainBus.Configuration;
using DomainBus.Dispatcher.Server;
using DomainBus.Sql.Communicators;
using DomainBus.Sql.Server;
using SqlFu;
using SqlFu.Builders;
using SqlFu.Configuration;
using SqlFu.Providers.SqlServer;

namespace DomainBus.Sql.SqlServer
{
    public static class Extensions
    {

        static IDbFactory GetFactory(string cnxString)
        {
            cnxString.MustNotBeEmpty("We need a valid connection string");
            var provider = new SqlServer2012Provider(SqlClientFactory.Instance.CreateConnection);
            return new DbFactory(new DbAccessProfile()
            {
                ConnectionString = cnxString,
                Name = "dbussql",
                Provider = provider
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="cnxString"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static IConfigureHost WithSqlServerStorages(this IConfigureHost host,string cnxString,Action<StoragesConfiguration> cfg)
        {
            return host.WithSqlStorages(GetFactory(cnxString), cfg);
        }

        public static void WithSqlServerStorage(this DispatchServerConfiguration cfg, string cnxString,string dbSchema="")
        {
            cfg.WithSqlStorage(GetFactory(cnxString), dbSchema);
        }

        public static void ReceiveFromClientsBySqlServer(this DispatchServerConfiguration cfg, string cnxString, string table = Sql.Extensions.CommunicatorTable, string dbSchema = Sql.Extensions.CommunicatorSchema)
        {
            cfg.ReceiveFromClientsBySql(GetFactory(cnxString),table,dbSchema);
        }

        public static IConfigureDispatcher CommunicateBySqlServerStorage(this IConfigureDispatcher cfg, string cnxString, string table = Sql.Extensions.CommunicatorTable, string dbSchema = Sql.Extensions.CommunicatorSchema, TableExistsAction ifExists = TableExistsAction.Ignore)
        {
            return cfg.CommunicateBySqlStorage(GetFactory(cnxString), table, dbSchema, ifExists);
        }
    }
}