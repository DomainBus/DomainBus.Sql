using System;
using System.Data.Common;
using DomainBus.Processing;
using SqlFu;

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
                return _db.RetryOnTransientError(db =>
                {
                    var data = db.WithSql(q => q.From<SagaRow>()
                        .Where(d => d.SagaId == SagaRow.GetId(correlationId, sagaStateType))
                        .Select(d => d.Data)).GetValue();
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
                _db.RetryOnTransientError(db =>
                {
                    if (isNew) Insert(db.Connection, data, correlationId);
                    else Update(db.Connection, data, correlationId);


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
                .Set(d => d.Data, data.Serialize().ToByteArray())
                .Set(d => d.Version,data.AutoTimestamp)
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
                    Data = data.Serialize().ToByteArray(),
                    IsCompleted = false,
                    LastChangedOn = DateTime.UtcNow,
                    Version = data.AutoTimestamp
                });
            }
            catch (DbException ex)
            {
                if (db.IsUniqueViolation(ex)) throw new SagaExistsException();
             
            }
        }
    }
}