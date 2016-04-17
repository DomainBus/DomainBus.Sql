using SqlFu;
using SqlFu.Builders.CreateTable;
using SqlFu.Providers.SqlServer;

namespace DomainBus.Sql.Processor
{
    public class ProcessorMessagesRowCreator : ATypedStorageCreator<ProcessorMessagesRow>
    {
        public ProcessorMessagesRowCreator(IDbFactory db) : base(db)
        {
        }

        protected override void Configure(IConfigureTable<ProcessorMessagesRow> cfg)
        {
            cfg.ColumnSize(d => d.Processor, 75)
                .PrimaryKey(pk=>pk.OnColumns(d=>d.Processor,d=>d.Id));
            if (_db.Provider.IsSqlserver()) cfg.ColumnSize(d => d.Data, "max");

        }
    }
}