using System.Collections.Generic;
using DomainBus.Dispatcher.Client;
using DomainBus.Sql.Server;
using FluentAssertions;

using Xunit;

namespace Tests
{
    public class ServerStateStorageTests
    {
        private ServerStateStorage _sut;

        public ServerStateStorageTests()
        {
            ServerStateStorage.Init(Setup.GetConnection());
            _sut=new ServerStateStorage(Setup.GetConnection());
        }

        [Fact]
        public void load_state()
        {
            var state=_sut.Load();
            state.Should().NotBeNull();
            var configs = new[] { new EndpointMessagesConfig() { Endpoint = DomainBus.Configuration.EndpointId.TestValue } };
            state.Update(configs);
            _sut.Save(state);

            var load = _sut.Load();
            load.ShouldBeEquivalentTo(state);
            load.Items.Should().NotBeEmpty();
        }

    }
}