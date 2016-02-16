using SqlFu;
using SqlFu.Builders.CreateTable;

namespace DomainBus.Sql.ReservedIds
{
    public class ReservedIdRowCreator : ATypedStorageCreator<ReservedIdRow>
    {
        public ReservedIdRowCreator(IDbFactory db) : base(db)
        {
        }

        protected override void Configure(IConfigureTable<ReservedIdRow> cfg)
        {
            cfg.PrimaryKey(pk => pk.OnColumns(d => d.Id))
                .ColumnSize(d=>d.Id,500)
                .ColumnSize(d => d.Data, 4000);
        }
    }
}