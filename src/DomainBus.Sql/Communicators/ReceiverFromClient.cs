using System.Collections.Generic;
using System.Linq;
using DomainBus.Transport;
using SqlFu;


namespace DomainBus.Sql.Communicators
{
    public class ReceiverFromClient:AClientMessagesReceiver
        {
        private readonly IDbFactory _db;
        
    
        protected override EnvelopeFromClient[] GetMessages()
        {
            IEnumerable<ClientToServerRow> items = Enumerable.Empty<ClientToServerRow>();
            _db.HandleTransientErrors(db =>
            {
                items = db.QueryAs(q => q.From<ClientToServerRow>()
                .Where(d=>d.Type==(int)ClientMessageType.Envelope)
                .OrderBy(d => d.Id)
                .SelectAll());
            }, 15, 150);
            return items.Select(d => d.Data.Deserialize<EnvelopeFromClient>()).ToArray();
        }

        protected override void MarkAsHandled(EnvelopeFromClient envs)
        {
            _db.HandleTransientErrors(
                db => db.DeleteFrom<ClientToServerRow>(t => t.DataId==envs.Id.ToString() && t.Type==(int)ClientMessageType.Envelope));
        }

     

        public ReceiverFromClient(IDbFactory db)
        {
            _db = db;            
        }

        
    }
}