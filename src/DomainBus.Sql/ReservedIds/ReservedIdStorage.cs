using System;
using System.Data.Common;
using System.Linq;
using DomainBus.Dispatcher;
using SqlFu;

namespace DomainBus.Sql.ReservedIds
{
    public class ReservedIdStorage:IStoreReservedMessagesIds
    {
        private readonly IDbFactory _db;

        public ReservedIdStorage(IDbFactory db)
        {
            _db = db;
        }

        public Guid[] Get(ReservedIdsSource input) 
            => _db.RetryOnTransientError(
                db => db.WithSql(
                    q => q.From<ReservedIdRow>()
                        .Where(d => d.Id == ReservedIdRow.GetId(input))
                        .Select(d => d.Data)
                ).GetValue()
                ?.Split(',').Select(Guid.Parse).ToArray() 
                ?? Array.Empty<Guid>());

        public void Add(ReservedIdsSource id, Guid[] ids)
        {
            _db.RetryOnTransientError(db =>
            {
                try
                {
                    db.Connection.Insert(new ReservedIdRow()
                    {
                        Id = ReservedIdRow.GetId(id),
                        Data = ids.Select(d => d.ToString()).StringJoin()
                    });
                }
                catch (DbException ex) when (db.Connection.IsUniqueViolation(ex))
                {
                    //ignore duplicates
                }
            });
        }
    }
}