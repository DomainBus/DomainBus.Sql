using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainBus.Configuration;
using DomainBus.Dispatcher.Client;
using DomainBus.Dispatcher.Server;
using DomainBus.Sql;
using DomainBus.Sql.Communicators;
using DomainBus.Transport;
using FluentAssertions;
using NSubstitute;

using SqlFu.Builders;
using Xunit;

namespace Tests
{

    public class FakeServer:IWantEndpointUpdates,IRouteMessages
    {
        
        public int Routed { get; private set; }
        public int Configs { get; private set; }

        public Task Route(EnvelopeFromClient envelope)
        {
            envelope.MustNotBeNull();
            Routed++;
            return Task.WhenAll();
        }


        public void ReceiveConfigurations(IEnumerable<EndpointMessagesConfig> update)
        {
            update.MustNotBeEmpty();
            Configs++;
        }
    }
    public class ClientToServerTests:IDisposable
    {
        private IConfigureDispatcher _disp;
        private ITalkToServer _clientToServer;
        private DispatchServerConfiguration _cfg;
        
        private IWantEndpointUpdates _serverConfig;
        private IRouteMessages _serverRoute;
     

        public ClientToServerTests()
        {
            _disp = Substitute.For<IConfigureDispatcher>();
            _disp.TalkUsing(Arg.Do<ITalkToServer>(v => _clientToServer = v));
            _disp.CommunicateBySqlStorage(Setup.GetConnection(), ifExists: TableExistsAction.DropIt);

            _cfg = new DispatchServerConfiguration();
            _cfg.ReceiveFromClientsBySql(Setup.GetConnection());

           

            _serverConfig = Substitute.For<IWantEndpointUpdates>();
            _serverRoute = Substitute.For<IRouteMessages>();

            _cfg.Storage = Substitute.For<IStoreDispatcherServerState>();
            _cfg.DeliveryErrorsQueue = Substitute.For<IDeliveryErrorsQueue>();

        

            _cfg.EndpointUpdatesNotifier.Subscribe(_serverConfig);
            _cfg.MessageNotifier.Subscribe(_serverRoute);


        }

        [Fact]
        public async Task send_receive_endpoint_config()
        {
          var  configs = new[] { new EndpointMessagesConfig()
             {
                 Endpoint = EndpointId.TestValue,
                 MessageTypes = new [] {typeof(MyEvent).AsMessageName()}
             }};
            _clientToServer.SendEndpointsConfiguration(configs);
           _cfg.EndpointUpdatesNotifier.Start();
          await Task.Delay(100).ConfigureFalse();
       
            _serverConfig.ReceivedWithAnyArgs(1).ReceiveConfigurations(configs);
            _serverConfig.ReceivedCalls().First().GetArguments()[0].ShouldBeEquivalentTo(configs);
        }

        [Fact]
        public async Task send_receive_client_messages()
        {
            var envelope = new EnvelopeFromClient()
            {
                From = "local",
                Id = Guid.NewGuid(),
                Messages = new[] {new MyEvent() }
            };
            await
                _clientToServer.SendMessages(envelope);
            _cfg.MessageNotifier.Start();
          await Task.Delay(100).ConfigureFalse();
            await _serverRoute.ReceivedWithAnyArgs(1).Route(null);
            _serverRoute.ReceivedCalls().First().GetArguments()[0].ShouldBeEquivalentTo(envelope);
          
        }

        public void Dispose()
        {
            (_cfg.EndpointUpdatesNotifier as IDisposable)?.Dispose();
            (_cfg.MessageNotifier as IDisposable)?.Dispose();
        }
    }
}