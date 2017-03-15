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

        public const string Table = "dbus_msg_storage";
        public const string Schema = "";
        public ProcessorStore(IDbFactory db)
        {
            _db = db;
        }

        public Task Add(string queueId, IEnumerable<IMessage> items)
        {
            try
            {
                return _db.RetryOnTransientErrorAsync(CancellationToken.None, async (db) =>
            {
                foreach (var msg in items)
                {
                    using (var t = db.Connection.BeginTransaction())
                    {
                        try
                        {
                            await
                                db.Connection.InsertAsync(new IdemRow() {MessageId = msg.Id + queueId}, db.Cancel)
                                    .ConfigureFalse();
                            await db.Connection.InsertAsync(new ProcessorMessagesRow()
                            {
                                Id = msg.Id,
                                ArrivalId = msg.TimeStamp.Ticks,
                                Processor = queueId,
                                Data = msg.Serialize().ToByteArray()
                            }, db.Cancel).ConfigureFalse();

                            t.Commit();
                        }
                        catch (DbException ex) when (db.Connection.IsUniqueViolation(ex))
                        {
                            //ignore, it's duplicate
                        }
                    }
                }
               
            });
            }
            catch (DbException ex)
            {
               throw new BusStorageException("Adding messages to storage failed", ex);
            }
        }

        public IEnumerable<IMessage> GetMessages(string queueId, int take)
        {
            try
            {
                return _db.RetryOnTransientError(db => 
                db.WithSql(q =>
                    q.From<ProcessorMessagesRow>()
                        .Where(d => d.Processor == queueId)
                        .OrderBy(d => d.ArrivalId)
                        .Limit(take)
                        .Select(d => d.Data))
                    .GetRows()
                    .Select(d => d.Deserialize<IMessage>())
                    .ToArray());
            }
            catch (DbException ex)
            {
                throw new BusStorageException("", ex);
            }
        }

        public void MarkMessageHandled(string queue, Guid id)
        {
            try
            {
                _db.RetryOnTransientError(
                    db => db.Connection.DeleteFrom<ProcessorMessagesRow>(d => d.Processor == queue && d.Id == id));
            }
            catch (DbException ex)
            {
                throw new BusStorageException("",ex);
            }
        }
    }
}