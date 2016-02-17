using DomainBus.Configuration;
using DomainBus.Sql.Processor;
using SqlFu;
using SqlFu.Builders;

namespace DomainBus.Sql
{
    public class StoragesConfiguration
    {
        private readonly IConfigureHost _host;
        private IDbFactory _factory;
        public const string ProcessorTable = "dbus_msg_storage";
        public const string ProcessorSchema = "";

        public StoragesConfiguration(IConfigureHost host, IDbFactory factory)
        {
            _host = host;
            _factory = factory;
        }


        public StoragesConfiguration EnableProcessorStorage(string tableName=ProcessorTable,string dbSchema=ProcessorSchema,TableExistsAction ifExists=TableExistsAction.Ignore)
        {
            new IdemRowCreator(_factory).WithTableName("dbus_idems", dbSchema).Create();
            new ProcessorMessagesRowCreator(_factory).WithTableName(tableName,dbSchema).IfExists(ifExists).Create();
            _host.WithProcessingStorage(new ProcessorStore(_factory));
            return this;
        }
    }
}