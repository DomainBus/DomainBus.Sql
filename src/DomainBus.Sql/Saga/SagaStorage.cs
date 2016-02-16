using System;
using System.Data.Common;
using DomainBus.Processing;
using SqlFu;
using SqlFu.Builders.CreateTable;

namespace DomainBus.Sql.Saga
{
    public class SagaStorage:IStoreSagaState
    {
        private readonly IDbFactory _db;

        public SagaStorage(IDbFactory db)
        {
            _db = db;
        }

        public ISagaState GetSaga(string correlationId, Type sagaStateType)
        {
            try
            {
                return _db.HandleTransientErrors(db =>
                {
                    var data = db.QueryValue(q => q.From<SagaRow>()
                        .Where(d => d.SagaId == SagaRow.GetId(correlationId, sagaStateType))
                        .Select(d => d.Data));
                    return data?.Deserialize<ISagaState>();
                });
            }
            catch (DbException ex)
            {
                throw new BusStorageException("",ex);
                
            }
        }




        public void Save(ISagaState data, string correlationId, bool isNew)
        {
            try
            {
                _db.HandleTransientErrors(db =>
                {
                    if (isNew) Insert(db, data, correlationId);
                    else Update(db, data, correlationId);


                });
            }
            catch (DbException ex)
            {
                throw new BusStorageException("", ex);

            }
        }

        private static void Update(DbConnection db, ISagaState data, string correlationId)
        {
            var old = data.AutoTimestamp;
            var result=db.Update<SagaRow>()
                .Set(d => d.Data, data.Serialize())
                .Set(d => d.Version, DateTime.UtcNow.Ticks)
                .Set(d => d.LastChangedOn, DateTime.UtcNow)
                .Set(d => d.IsCompleted, data.IsCompleted)
                .Where(d => d.SagaId == SagaRow.GetId(correlationId, data.GetType()) && d.Version == old)
                .Execute();
            if (result!=1) throw new SagaConcurrencyException();

        }

        private static void Insert(DbConnection db, ISagaState data, string correlationId)
        {
            try
            {
                db.Insert(new SagaRow()
                {
                    SagaId = SagaRow.GetId(correlationId, data.GetType()),
                    Data = data.Serialize(),
                    IsCompleted = false,
                    LastChangedOn = DateTime.UtcNow,
                    Version = DateTime.UtcNow.Ticks
                });
            }
            catch (DbException ex)
            {
                if (db.IsUniqueViolation(ex)) throw new SagaExistsException();
             
            }
        }
    }

    public class SagaRow
    {
        public string SagaId { get; set; }
        public string Data { get; set; }
        public long Version { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime LastChangedOn { get; set; }

        public static string GetId(string correlation, Type type)
            => (correlation + type.FullName).MurmurHash().ToBase64();
    }


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