using SqlFu;
using SqlFu.Builders.CreateTable;

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
                .ColumnSize(d => d.Data, "max")
                .PrimaryKey(pk => pk.OnColumns(d => d.SagaId));
        }
    }
}