using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainBus.Abstractions;
using DomainBus.Processing;
using SqlFu;

namespace DomainBus.Sql.Processor
{
    public class ProcessorStore:IStoreUnhandledMessages
    {
        private readonly IDbFactory _db;

        public ProcessorStore(IDbFactory db)
        {
            _db = db;
        }

        public Task Add(string queueId, IEnumerable<IMessage> items)
        {
            return _db.HandleTransientErrorsAsync(CancellationToken.None, async (db, tok) =>
            {
                foreach (var msg in items)
                {
                    using (var t = db.BeginTransaction())
                    {
                        try
                        {
                            await db.InsertAsync(new IdemRow() {MessageId = msg.Id + queueId},tok);
                            await db.InsertAsync(new ProcessorMessagesRow()
                            {
                                Id=msg.Id,
                                ArrivedAt = msg.TimeStamp.UtcDateTime,
                                Processor = queueId,
                                Data=msg.Serialize()
                            }, tok);

                            t.Commit();
                        }
                        catch (DbException ex)
                        {
                            if (db.IsUniqueViolation(ex)) return;
                            throw new BusStorageException("Adding messages to storage failed", ex);
                        }
                    }
                }
               
            });
        }

        public IEnumerable<IMessage> GetMessages(string queueId, int take)
        {
            return _db.HandleTransientErrors(db =>
            {
                try
                {
                    return db.QueryAs(q =>
                        q.From<ProcessorMessagesRow>()
                            .Where(d => d.Processor == queueId)
                            .Limit(take)
                            .Select(d => d.Data)
                        ).Select(d => d.Deserialize<IMessage>());
                }
                catch (DbException ex)
                {
                    throw new BusStorageException("",ex);
                }
            });
        }

        public void MarkMessageHandled(string queue, Guid id)
        {
            try
            {
                _db.HandleTransientErrors(
                    db => db.DeleteFrom<ProcessorMessagesRow>(d => d.Processor == queue && d.Id == id));
            }
            catch (DbException ex)
            {
                throw new BusStorageException("",ex);
            }
        }
    }
}