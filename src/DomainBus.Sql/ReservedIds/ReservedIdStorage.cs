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
        {
            return _db.HandleTransientErrors(db =>
            {
                var data=db.QueryValue(q => q.From<ReservedIdRow>()
                .Where(d=>d.Id==ReservedIdRow.GetId(input))
                .Select(d=>d.Data)
                );
                return data?.Split(',').Select(Guid.Parse).ToArray()??Array.Empty<Guid>();
            });
        }

        public void Add(ReservedIdsSource id, Guid[] ids)
        {
            _db.HandleTransientErrors(db =>
            {
                try
                {
                    db.Insert(new ReservedIdRow()
                    {
                        Id = ReservedIdRow.GetId(id),
                        Data = ids.Select(d => d.ToString()).StringJoin()
                    });
                }
                catch (DbException ex) when (db.IsUniqueViolation(ex))
                {
                    //ignore duplicates
                }
            });
        }
    }
}