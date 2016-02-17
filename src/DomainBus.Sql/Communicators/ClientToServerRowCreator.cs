using SqlFu;
using SqlFu.Builders.CreateTable;

namespace DomainBus.Sql.Communicators
{
    public class ClientToServerRowCreator : ATypedStorageCreator<ClientToServerRow>
    {
        public ClientToServerRowCreator(IDbFactory db) : base(db)
        {
        }

        protected override void Configure(IConfigureTable<ClientToServerRow> cfg)
        {
            cfg.Column(d => d.Id, c => c.AutoIncrement())
                .ColumnSize(d => d.Data, "max")
                .ColumnSize(d=>d.DataId,150)
                .PrimaryKey(pk=>pk.OnColumns(d=>d.Id))
                ;
        }
    }
}