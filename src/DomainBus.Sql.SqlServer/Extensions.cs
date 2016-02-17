using System;
using System.Data.SqlClient;
using DomainBus.Configuration;
using SqlFu;
using SqlFu.Configuration;
using SqlFu.Providers.SqlServer;

namespace DomainBus.Sql.SqlServer
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="cnxString"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static IConfigureHost WithSqlServerStorages(this IConfigureHost host,string cnxString,Action<StoragesConfiguration> cfg)
        {
            cnxString.MustNotBeEmpty("We need a valid connection string");
            var provider=new SqlServer2012Provider(SqlClientFactory.Instance.CreateConnection);
            return host.WithSqlStorages(new DbFactory(new DbAccessProfile()
            {
                ConnectionString = cnxString,
                Name = "dbussql",
                Provider = provider
            }), cfg);
        }
    }
}