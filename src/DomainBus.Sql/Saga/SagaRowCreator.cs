using SqlFu;
using SqlFu.Builders.CreateTable;
using SqlFu.Providers.SqlServer;

namespace DomainBus.Sql.Saga
{
    public class SagaRowCreator : ATypedStorageCreator<SagaRow>
    {
        public SagaRowCreator(IDbFactory db) : base(db)
        {
        }

        protected override void Configure(IConfigureTable<SagaRow> cfg)
        {
            cfg.Column(c => c.SagaId, c => c.HasSize(32).NotNull())
                .PrimaryKey(pk => pk.OnColumns(d => d.SagaId));
            if (_db.Provider.IsSqlserver()) cfg.ColumnSize(d => d.Data, "max");
        }
    }
}