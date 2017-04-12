using SqlFu;
using SqlFu.Builders.CreateTable;
using SqlFu.Providers.Sqlite;
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
                .Column(d => d.ArrivalId, c => c.AutoIncrement())

                .Index(pk=>pk.OnColumns(d=>d.MessageId,d=>d.Processor).Unique());
            if (this._db.Provider.IsSqlite())
            {
                cfg.ColumnDbType(d => d.MessageId, SqliteType.Text)

                    //.PrimaryKey(pk => pk.OnColumns(d => d.ArrivalId))
                    ;

            }
            if (this._db.Provider.IsSqlserver())
            {
                cfg.ColumnSize(d => d.Data, "max")
                    .PrimaryKey(pk => pk.OnColumns(d => d.ArrivalId))
                    //.Column(d => d.ArrivalId, c => c.AutoIncrement())
                ;
            }                                
        }
    }
}