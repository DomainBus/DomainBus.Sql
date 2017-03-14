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
                
                .PrimaryKey(pk=>pk.OnColumns(d=>d.Processor,d=>d.Id));
            if (this._db.Provider.IsSqlite())
            {
                cfg.Column(d => d.ArrivalId, c => c.HasDbType("timestamp").HasDefaultValue("CURRENT_TIMESTAMP"))
                    .ColumnDbType(d => d.Id, SqliteType.Text)
                    ;
            }
            if (this._db.Provider.IsSqlserver())
            {
                cfg.Column(d => d.ArrivalId, c => c.HasDbType(SqlServerType.DateTime).HasDefaultValue("getdate()",true));
                cfg.ColumnSize(d => d.Data, "max");
            }                                
        }
    }
}