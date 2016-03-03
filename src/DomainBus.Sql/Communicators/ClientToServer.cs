using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainBus.Dispatcher.Client;
using DomainBus.Transport;
using SqlFu;

namespace DomainBus.Sql.Communicators
{
    public class ClientToServer:ITalkToServer
    {
        private readonly IDbFactory _db;

        public ClientToServer(IDbFactory db)
        {
            _db = db;
        }

        public void SendEndpointsConfiguration(IEnumerable<EndpointMessagesConfig> data)
        {
            _db.HandleTransientErrors(db =>
            {
                using (var t = db.BeginTransaction())
                {
                    foreach (var config in data)
                    {
                        var row = new ClientToServerRow()
                        {
                            Type = ClientMessageType.EndpointConfig,
                            DataId = config.Endpoint,
                            Data = config.Serialize().ToByteArray()
                        };
                        db.Insert(row);
                    }
                    t.Commit();
                }
            });
        }

        /// <exception cref="CouldntSendMessagesException"></exception>
        public async Task SendMessages(EnvelopeFromClient envelope)
        {
            try
            {
                await _db.HandleTransientErrorsAsync(CancellationToken.None, (db, tok) =>

                    db.InsertAsync(new ClientToServerRow()
                    {
                        DataId = envelope.Id.ToString(),
                        Type = ClientMessageType.Envelope,
                        Data = envelope.Serialize().ToByteArray()
                    }, tok), wait: 200).ConfigureFalse();
            }
            catch (DbException ex)
            {
                throw new CouldntSendMessagesException(envelope,"Error adding to storage",ex);
            }
        }
    }
}