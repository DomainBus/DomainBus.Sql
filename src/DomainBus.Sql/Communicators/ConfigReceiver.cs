using System;
using System.Collections.Generic;
using System.Linq;
using DomainBus.Dispatcher.Client;
using DomainBus.Transport;
using SqlFu;
using SqlFu.Builders;

namespace DomainBus.Sql.Communicators
{
    public class ConfigReceiver : AEndpointConfigurationReceiver
    {
        private readonly IDbFactory _db;

        public ConfigReceiver(IDbFactory db)
        {
            _db = db;
            Timer.Interval=TimeSpan.FromMinutes(5);
        }

        protected override EndpointMessagesConfig[] GetConfigs()
        =>

            _db.HandleTransientErrors(db =>
            
                db.QueryAs(q=>q.From<ClientToServerRow>().Where(d=>d.Type==ClientMessageType.EndpointConfig)
                .Select(d=>d.Data)).FilterNulls().Select(d=>d.Deserialize<EndpointMessagesConfig>())
                .ToArray()
            );

        

        protected override void MarkAsHandled(IEnumerable<EndpointMessagesConfig> configs)
        {
            var values = configs.Select(d => d.Endpoint.ToString());
            _db.HandleTransientErrors(
              db => db.DeleteFrom<ClientToServerRow>(t 
              => t.DataId.HasValueIn(values)&&t.Type == ClientMessageType.EndpointConfig));
        }
    }
}