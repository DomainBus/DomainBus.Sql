using SqlFu;
using SqlFu.Builders.CreateTable;

namespace DomainBus.Sql.Processor
{
    public class IdemRowCreator : ATypedStorageCreator<IdemRow>
    {
        public IdemRowCreator(IDbFactory db) : base(db)
        {
        }

        protected override void Configure(IConfigureTable<IdemRow> cfg)
        {
           
                cfg.ColumnSize(d => d.MessageId, 150);
                
                cfg.PrimaryKey(pk => pk.OnColumns(d => d.MessageId));
           
            
                
        }
    }
}