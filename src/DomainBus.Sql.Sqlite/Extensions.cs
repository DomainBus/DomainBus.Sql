using System;
using System.Data.Common;
#if NET461
using System.Data.SQLite;
#endif
#if NETCORE
using Microsoft.Data.Sqlite;
#endif
using DomainBus.Configuration;
using DomainBus.Dispatcher.Server;
using SqlFu;
using SqlFu.Builders;
using SqlFu.Configuration;
using SqlFu.Providers.Sqlite;

namespace DomainBus.Sql.Sqlite
{
    public static class Extensions
    {

        static IDbFactory GetFactory(string cnxString)
        {      
            cnxString.MustNotBeEmpty("We need a valid connection string");


#if NET461
            Func<DbConnection> cnx = SQLiteFactory.Instance.CreateConnection;
#endif
#if NETCORE
            Func<DbConnection> cnx = SqliteFactory.Instance.CreateConnection;
#endif
            var provider = new SqliteProvider(cnx);
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
        public static IConfigureHost WithSqliteStorages(this IConfigureHost host,string cnxString,Action<StoragesConfiguration> cfg)
        {
            return host.WithSqlStorages(GetFactory(cnxString), cfg);
        }

        public static void WithSqliteStorage(this DispatchServerConfiguration cfg, string cnxString,string dbSchema="")
        {
            cfg.WithSqlStorage(GetFactory(cnxString), dbSchema);
        }

        public static void ReceiveFromClientsBySqlite(this DispatchServerConfiguration cfg, string cnxString, string table = Sql.Extensions.CommunicatorTable, string dbSchema = Sql.Extensions.CommunicatorSchema)
        {
            cfg.ReceiveFromClientsBySql(GetFactory(cnxString),table,dbSchema);
        }

        public static IConfigureDispatcher CommunicateBySqliteStorage(this IConfigureDispatcher cfg, string cnxString, string table = Sql.Extensions.CommunicatorTable, string dbSchema = Sql.Extensions.CommunicatorSchema, TableExistsAction ifExists = TableExistsAction.Ignore)
        {
            return cfg.CommunicateBySqlStorage(GetFactory(cnxString), table, dbSchema, ifExists);
        }
    }
}