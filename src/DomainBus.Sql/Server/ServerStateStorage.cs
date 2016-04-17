using DomainBus.Dispatcher.Server;
using SqlFu;
using SqlFu.Builders.CreateTable;
using SqlFu.Providers.SqlServer;

namespace DomainBus.Sql.Server
{
    public class ServerStateStorage:IStoreDispatcherServerState
    {
        private readonly IDbFactory _db;

        public ServerStateStorage(IDbFactory db)
        {
            _db = db;
        }

        public DispatcherState Load()
        {
            return _db.HandleTransientErrors(db =>
            {
                return db.QueryValue(q => q.From<ServerStateItem>().Select(d=>d.Data))
                    ?.Deserialize<DispatcherState>();
            });
        }

        public void Save(DispatcherState state)
        {
            _db.HandleTransientErrors(db =>
            {
                db.Update<ServerStateItem>()
                    .Set(d => d.Data, state.Serialize())
                    .Execute();
            },wait:300);
        }

        public static void Init(IDbFactory fac,string dbSchema="")
        {
            using (var db = fac.Create())
            {
                db.CreateTableFrom<ServerStateItem>(c =>
                {
                    c.TableName("dbus_server_state", dbSchema);
                    c.IgnoreIfExists();
                    if (db.Provider().IsSqlserver())c.ColumnSize(d => d.Data, "max");
                });
                db.Insert(new ServerStateItem() {Data = new DispatcherState().Serialize()});
            }
        }
    }

    public class ServerStateItem
    {
        public string Data { get; set; }
    }


}