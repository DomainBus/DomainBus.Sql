using DomainBus.Configuration;
using DomainBus.Dispatcher.Client;
using DomainBus.Dispatcher.Server;
using DomainBus.Sql;
using FluentAssertions;
using NSubstitute;
using SqlFu.Builders;
using Xunit;

namespace Tests
{
    public class ClientToServerTests
    {
        private IConfigureDispatcher _disp;
        private ITalkToServer _clientToServer;
        private DispatchServerConfiguration _cfg;
        private EndpointMessagesConfig[] _configs;
        private IWantEndpointUpdates _serverConfig;
        private IRouteMessages _serverRoute;

        public ClientToServerTests()
        {
            _disp = Substitute.For<IConfigureDispatcher>();
            _disp.TalkUsing(Arg.Do<ITalkToServer>(v => _clientToServer = v));
            _disp.CommunicateBySqlStorage(Setup.GetConnection(), ifExists: TableExistsAction.DropIt);

            _cfg = new DispatchServerConfiguration();
            _cfg.ReceiveFromClientsBySql(Setup.GetConnection());

            _configs = new[] { new EndpointMessagesConfig()
             {
                 Endpoint = EndpointId.TestValue,
                 MessageTypes = new [] {typeof(MyEvent).AsMessageName()}
             }};

            _serverConfig = Substitute.For<IWantEndpointUpdates>();
            _serverRoute = Substitute.For<IRouteMessages>();

            _cfg.EndpointUpdatesNotifier.Subscribe(_serverConfig);
            _cfg.MessageNotifier.Subscribe(_serverRoute);
        }

        [Fact]
        public void send_receive_endpoint_config()
        {
            _clientToServer.SendEndpointsConfiguration(_configs);
            _cfg.EndpointUpdatesNotifier.Start();
            _serverConfig.Received(1).ReceiveConfigurations(_configs);
        }

    }
}