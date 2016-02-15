using SqlFu;
using SqlFu.Builders.CreateTable;

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

            cfg.ColumnSize(d => d.Data, "max");

        }
    }
}